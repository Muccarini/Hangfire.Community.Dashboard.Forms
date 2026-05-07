using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Hangfire.Community.Dashboard.Forms.Metadata;
using Hangfire.Community.Dashboard.Forms.Pages.Partials;
using Hangfire.Community.Dashboard.Forms.Support;


namespace Hangfire.Community.Dashboard.Forms.Partials
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
				if (!nAllowedTypes.Add(type)) { return "<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> Circular reference detected, not allowed.</div>"; }

				inputTMP += $@"
				<section class=""panel panel-default hdm-object-panel"" aria-labelledby=""heading_{id}"">
					<header class=""panel-heading hdm-object-header collapsed"" id=""heading_{id}"" role=""button"" tabindex=""0"" data-toggle=""collapse"" href=""#collapse_{id}"" aria-expanded=""false"" aria-controls=""collapse_{id}"">
						<h4 class=""panel-title hdm-object-title"">
							<span class=""glyphicon glyphicon-th hdm-object-icon"" aria-hidden=""true""></span>
							{System.Net.WebUtility.HtmlEncode(labelText)}
						</h4>
					</header>
					<div id=""collapse_{id}"" class=""panel-collapse collapse"" role=""region"" aria-labelledby=""heading_{id}"">
						<div class=""panel-body hdm-object-body"">";

				foreach (var propertyInfo in type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
				{
					var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
					propDisplayInfo.Label = propDisplayInfo.Label ?? propertyInfo.Name;
					var propDefaultValue = isLoaded ? defaultValue?.GetType().GetProperty(propertyInfo.Name)?.GetValue(defaultValue) : propDisplayInfo.DefaultValue;
					string propId = $"{id}_{propertyInfo.Name}";

					inputTMP += ToHtml(propertyInfo.PropertyType, propId, propDisplayInfo, listDepth, propDefaultValue, nAllowedTypes);
				}

				nAllowedTypes.Remove(type);

				return inputTMP += @"
						</div>
					</div>
				</section>";
			}

			if (type.IsInterface)
			{
				if (!VT.Implementations.ContainsKey(type)) { return $"<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> No concrete implementation of \"{System.Net.WebUtility.HtmlEncode(type.Name)}\" found in the current assembly.</div>"; }

				var impls = VT.Implementations[type];

				if (impls == null || impls.Count < 1)
				{
					return $"<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> No concrete implementation of \"{System.Net.WebUtility.HtmlEncode(type.Name)}\" found in the current assembly.</div>";
				}

				if (impls.Count == 1)
				{
					var implType = impls.First();
					var implDisplayName = VT.GetDisplayName(implType);
					var encodedImplFullName = System.Net.WebUtility.HtmlEncode(implType.FullName);
					inputTMP += $@"
					<input type=""hidden"" id=""{id}"" class=""hdm-job-input hdm-input-datalist"" data-selectedvalue=""{encodedImplFullName}"" />
					<section class=""panel panel-default hdm-impl-panel"" aria-labelledby=""heading_{id}_{type.Name}"">
						<header class=""panel-heading hdm-impl-header collapsed"" id=""heading_{id}_{type.Name}"" role=""button"" tabindex=""0"" data-toggle=""collapse"" href=""#collapse_{id}_{type.Name}"" aria-expanded=""false"" aria-controls=""collapse_{id}_{type.Name}"">
							<h4 class=""panel-title hdm-impl-title"">
								<span class=""glyphicon glyphicon-th hdm-impl-icon"" aria-hidden=""true""></span>
								{System.Net.WebUtility.HtmlEncode(implDisplayName)}
							</h4>
						</header>
						<div id=""collapse_{id}_{type.Name}"" class=""panel-collapse collapse"" role=""region"" aria-labelledby=""heading_{id}_{type.Name}"">
							<div class=""panel-body hdm-impl-body"">";
					inputTMP += ToHtml(implType, $"{id}_{implType.Name}", displayInfo, listDepth, defaultValue, nAllowedTypes);
					inputTMP += @"
							</div>
						</div>
					</section>";
					return inputTMP;
				}
				else
				{
					var filteredImpls = new HashSet<Type>(impls.Where(impl => !nAllowedTypes.Contains(impl)));

					//currently default value for interface is not supported.
					Type defaultImplType = isLoaded ? defaultValue.GetType() : (Type)defaultValue ?? null;

					if (defaultImplType != null)
					{
						var defaultImplDisplayName = VT.GetDisplayName(defaultImplType);
						if (!type.IsAssignableFrom(defaultImplType)) { return $"<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> Default type \"{System.Net.WebUtility.HtmlEncode(defaultImplDisplayName)}\" does not implement interface \"{System.Net.WebUtility.HtmlEncode(type.Name)}\".</div>"; }
						if (!impls.Contains(defaultImplType)) { return $"<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> Default type \"{System.Net.WebUtility.HtmlEncode(defaultImplDisplayName)}\" is not in the list of implementations.</div>"; }
						if (!filteredImpls.Contains(defaultImplType)) { return $"<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> Default type \"{System.Net.WebUtility.HtmlEncode(defaultImplDisplayName)}\" creates a circular reference and is not allowed.</div>"; }
					}

					inputTMP += FormPartial.InputImplsMenu(id, displayInfo.CssClasses, labelText, displayInfo.Placeholder, displayInfo.Description, filteredImpls, defaultImplType, displayInfo.IsDisabled, displayInfo.IsRequired);

					//Concrete
					foreach (Type impl in filteredImpls)
					{
						if (!nAllowedTypes.Add(impl)) { return "<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> Circular reference detected, not allowed.</div>"; }

						var dNone = impl.IsEquivalentTo(defaultImplType) ? "" : "d-none";
						var implDisplayName = VT.GetDisplayName(impl);

						inputTMP += $@"
						<section id=""{id}_{impl.Name}"" class=""panel panel-default impl-panels-for-{id} hdm-impl-panel {dNone}"" aria-labelledby=""heading_{id}_{impl.Name}"">
							<header class=""panel-heading hdm-impl-header collapsed"" id=""heading_{id}_{impl.Name}"" role=""button"" tabindex=""0"" data-toggle=""collapse"" href=""#collapse_{id}_{impl.Name}"" aria-expanded=""false"" aria-controls=""collapse_{id}_{impl.Name}"">
								<h4 class=""panel-title hdm-impl-title"">
									<span class=""glyphicon glyphicon-th hdm-impl-icon"" aria-hidden=""true""></span>
									{System.Net.WebUtility.HtmlEncode(implDisplayName)}
									<span class=""hdm-interface-badge"">{System.Net.WebUtility.HtmlEncode(type.Name)}</span>
								</h4>
							</header>
							<div id=""collapse_{id}_{impl.Name}"" class=""panel-collapse collapse"" role=""region"" aria-labelledby=""heading_{id}_{impl.Name}"">
								<div class=""panel-body hdm-impl-body"">";

						foreach (var propertyInfo in impl.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
						{
							var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
							propDisplayInfo.Label = propDisplayInfo.Label ?? propertyInfo.Name;
							var propDefaultValue = isLoaded ? defaultValue?.GetType().GetProperty(propertyInfo.Name)?.GetValue(defaultValue) : propDisplayInfo.DefaultValue;
							string propId = $"{id}_{impl.Name}_{propertyInfo.Name}";

							inputTMP += ToHtml(propertyInfo.PropertyType, propId, propDisplayInfo, listDepth, propDefaultValue, nAllowedTypes);
						}

						nAllowedTypes.Remove(impl);

						inputTMP += @"
								</div>
							</div>
						</section>";
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
					if (defaultValueList.Count == 0)
					{
						return FormPartial.InputList(id, labelText, 0,
							FormPartial.InputElementList(0, listDepth, "d-none",
							ToHtml(genericArgument, $"{id}_list_0", displayInfo, listDepth + 1, defaultValue)));
					}

					return FormPartial.InputList(id, labelText, ((IList)defaultValue).Count, inputTMP);
				}

			}

			return "<div class=\"hdm-error-message\" role=\"alert\"><span class=\"glyphicon glyphicon-warning-sign\" aria-hidden=\"true\"></span> Unsupported type or not implemented yet.</div>";
		}
	}
}
