using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Hangfire.Dashboard.Management.Dynamic.Metadata;
using Hangfire.Dashboard.Management.Dynamic.Pages.Partials;
using Hangfire.Dashboard.Management.Dynamic.Support;


namespace Hangfire.Dashboard.Management.Dynamic.Partials
{
    public static class TypePartial
    {
        public static string ToHtml(Type type, string id, DisplayDataAttribute displayInfo, int listDepth, object defaultValue = null, HashSet<Type> nAllowedTypes = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (displayInfo == null) throw new ArgumentNullException(nameof(displayInfo));
			if (nAllowedTypes == null) nAllowedTypes = new HashSet<Type>();

			bool isGeneric = type.IsGenericType;
            bool isList = isGeneric && type.GetGenericTypeDefinition() == typeof(List<>);
            bool isNullable = isGeneric && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            bool isLoaded = defaultValue != null;

            string inputTMP = string.Empty;
            Type genericArgument = type.IsGenericType ? type.GetGenericArguments()[0] : null; //multiple generic arguments are not supported

            string labelText = displayInfo.Label ?? genericArgument?.Name ?? type.Name;
            string placeholderText = displayInfo.Placeholder ?? labelText;

            if (type == typeof(string))
            {
                return FormPartial.InputString(id, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, defaultValue, displayInfo.IsDisabled, displayInfo.IsRequired, displayInfo.IsMultiLine);
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                return FormPartial.InputInteger(id, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, defaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
            }
            else if (type == typeof(Uri))
            {
                return FormPartial.Input(id, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, "url", defaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return FormPartial.InputDateTime(id, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, defaultValue, displayInfo.IsDisabled, displayInfo.IsRequired, displayInfo.ControlConfiguration);
            }
            else if (type == typeof(bool))
            {
                return FormPartial.InputBoolean(id, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, defaultValue, displayInfo.IsDisabled);
            }
            else if (type.IsEnum || (isNullable && genericArgument.IsEnum))
            {
                var data = new Dictionary<string, int>();
                foreach (var name in Enum.GetNames(type.IsEnum ? type : genericArgument))
                {
                    var value = (int)Enum.Parse(type.IsEnum ? type : genericArgument, name);
                    data.Add(name, value);
                }

                return FormPartial.InputEnum(id, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, data, defaultValue?.ToString(), displayInfo.IsDisabled);
            }

            if (type.IsClass && !isGeneric)
            {
                if (!nAllowedTypes.Add(type)) { return "<span>Circular reference detected, not allowed.</span>"; } //Circular reference, not allowed -> null

                inputTMP += $"<div class=\"panel panel-default\"><div class=\"panel-heading\" role=\"button\" data-toggle=\"collapse\" href=\"#collapse_{id}\" aria-expanded=\"false\" aria-controls=\"collapse_{id}\"><h4 class=\"panel-title\">{labelText}</h4></div><div id=\"collapse_{id}\" class=\"panel-collapse collapse\"><div class=\"panel-body\">";

                foreach (var propertyInfo in type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
                {
                    var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
					propDisplayInfo.Label = propDisplayInfo.Label ?? propertyInfo.Name;
					var propDefaultValue = isLoaded ? defaultValue?.GetType().GetProperty(propertyInfo.Name)?.GetValue(defaultValue) : propDisplayInfo.DefaultValue;
                    string propId = $"{id}_{propertyInfo.Name}";

                    inputTMP += ToHtml(propertyInfo.PropertyType, propId, propDisplayInfo, listDepth, propDefaultValue, nAllowedTypes);
                }

				nAllowedTypes.Remove(type);

                return inputTMP += "</div></div></div>";
            }

            if (type.IsInterface)
            {
                if (!VT.Implementations.ContainsKey(type)) { return $"<span>No concrete implementation of \"{type.Name}\" found in the current assembly.</span>"; }

                var impls = VT.Implementations[type];

                if (impls == null || impls.Count < 1)
                {
                    return $"<span>No concrete implementation of \"{type.Name}\" found in the current assembly.</span>";
                }

                if (impls.Count == 1)
                {
                    var implType = impls.First();
                    inputTMP += $"<div class=\"panel panel-default\"><div class=\"panel-heading\" role=\"button\" data-toggle=\"collapse\" href=\"#collapse_{id}_{implType.Name}\" aria-expanded=\"false\" 	aria-controls=\"collapse_{id}_{implType.Name}\"><h4 class=\"panel-title\">{implType.Name}</h4></div><div id=\"collapse_{id}_{implType.Name}\" class=\"panel-collapse collapse\"><div class=\"panel-body\">";
                    inputTMP += ToHtml(implType, $"{id}_{implType.Name}", displayInfo, listDepth, defaultValue, nAllowedTypes);
                    inputTMP += "</div></div></div>";
                }
                else
                {
                    var filteredImpls = new HashSet<Type>(impls.Where(impl => !nAllowedTypes.Contains(impl)));

					//currently default value for interface is not supported.
					Type defaultImplType = isLoaded ? defaultValue.GetType() : (Type)defaultValue ?? null;

					if (defaultImplType != null)
                    {
                        if (!type.IsAssignableFrom(defaultImplType)) { return $"<span>Default type \"{defaultImplType.Name}\" does not implement interface \"{type.Name}\".</span>"; }
                        if (!impls.Contains(defaultImplType)) { return $"<span>Default type \"{defaultImplType.Name}\" is not in the list of implementations.</span>"; }
                        if (!filteredImpls.Contains(defaultImplType)) { return $"<span>Default type \"{defaultImplType.Name}\" creates a circular reference and is not allowed.</span>"; }
                    }

                    inputTMP += FormPartial.InputImplsMenu(id, displayInfo.CssClasses, labelText, displayInfo.Placeholder, displayInfo.Description, filteredImpls, defaultImplType, displayInfo.IsDisabled, displayInfo.IsRequired);

					//Concrete
                    foreach (Type impl in filteredImpls)
                    {
                        if (!nAllowedTypes.Add(impl)) { return "<span>Circular reference detected, not allowed.</span>"; } //Circular reference, not allowed -> null

                        var dNone = impl.IsEquivalentTo(defaultImplType) ? "" : "d-none";

                        inputTMP += $"<div id=\"{id}_{impl.Name}\" class=\"panel panel-default impl-panels-for-{id} {dNone}\"><div class=\"panel-heading\" role=\"button\" data-toggle=\"collapse\" href=\"#collapse_{id}_{impl.Name}\" aria-expanded=\"false\" aria-controls=\"collapse_{id}_{impl.Name}\"><h4 class=\"panel-title\">{impl.Name} | {type.Name}</h4></div><div id=\"collapse_{id}_{impl.Name}\" 	class=\"panel-collapse collapse\"><div 	class=\"panel-body\">";

                        foreach (var propertyInfo in impl.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
                        {
                            var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
				        	propDisplayInfo.Label = propDisplayInfo.Label ?? propertyInfo.Name;
				        	var propDefaultValue = isLoaded ? defaultValue?.GetType().GetProperty(propertyInfo.Name)?.GetValue(defaultValue) : propDisplayInfo.DefaultValue;
                            string propId = $"{id}_{impl.Name}_{propertyInfo.Name}";

                            inputTMP += ToHtml(propertyInfo.PropertyType, propId, propDisplayInfo, listDepth, propDefaultValue, nAllowedTypes);
                        }

				        nAllowedTypes.Remove(impl);

                        inputTMP += "</div></div></div>";
                    }

                    return inputTMP;
                }
            }

            if (isList)
            {
				//List<List<...<Concrete>>

				// - List Wrapper
				// -- Element index 0
				// --- Element Content
				// ...
				// -- Element index 1
				// --- Element Content

				if (!isLoaded)
                {

					labelText += " | Collection of ";
					var innerType = genericArgument;

					// Append "Collection of " to labelText for each nested generic List<> or Nullable<>
					while (innerType.IsGenericType)
                    {
                        if (innerType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            innerType = innerType.GetGenericArguments()[0];
                            labelText += $"Collections of ";
                        }
                        else if (innerType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            innerType = innerType.GetGenericArguments()[0];
                        }
                    }

					labelText += $"{innerType.Name}";
					displayInfo.Label = $"{innerType.Name} | Element";

					//if we are not loading a job, we can Depth-first visiting till we find a concrete type, then we process it as d-none element (FE will use it as template)
                    return FormPartial.InputList(id, labelText, 0,
                        FormPartial.InputElementList(0, listDepth, "d-none",
                            ToHtml(genericArgument, $"{id}_list_0", displayInfo, listDepth + 1, defaultValue)));
                }
                else
                {
                    //if we loading a job, Breadth-first visiting
                    IList defaultValueList = (IList)defaultValue;

					labelText += " | Collection of ";
					var innerType = genericArgument;

					// Append "Collection of " to labelText for each nested generic List<> or Nullable<>
					while (innerType.IsGenericType)
					{
						if (innerType.GetGenericTypeDefinition() == typeof(List<>))
						{
							innerType = innerType.GetGenericArguments()[0];
							labelText += $"Collections of ";
						}
						else if (innerType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							innerType = innerType.GetGenericArguments()[0];
						}
					}

					labelText += $"{innerType.Name}";
					displayInfo.Label = $"{innerType.Name} | Element";

					for (int i = 0; i < defaultValueList.Count; i++)
                    {
                        var elementValue = defaultValueList[i];

                        inputTMP += FormPartial.InputElementList(i, listDepth, "",
                            ToHtml(genericArgument, $"{id}_list_{i}", displayInfo, listDepth + 1, elementValue));
                    }

					//the template atleast
					if(defaultValueList.Count == 0)
					{
						return FormPartial.InputList(id, labelText, 0,
							FormPartial.InputElementList(0, listDepth, "d-none",
							ToHtml(genericArgument, $"{id}_list_0", displayInfo, listDepth + 1, defaultValue)));
					}

					return FormPartial.InputList(id, labelText, ((IList)defaultValue).Count, inputTMP);
                }

            }
            
            return inputTMP = "<span>Unsupported type or not implemented yet.</span>";
        }
    }   
}
