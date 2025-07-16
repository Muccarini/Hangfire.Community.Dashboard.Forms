using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Dashboard.Management.Dynamic.Pages.Partials
{
    public static class FormPartial
    {
        public static string InputList(string myId, string labelText, int length, string nestedElement)
        {
            return $@"
    				<div class=""panel panel-default"">
    					<div class=""panel-heading"" role=""button"" data-toggle=""collapse"" href=""#collapse_{myId}"" aria-expanded=""false"" aria-controls=""collapse_{myId}"">
    						<h4 class=""panel-title"">{labelText}</h4>
    					</div>
    					<div id=""collapse_{myId}"" class=""panel-collapse collapse"">
    						<div id=""{myId}"" class=""panel-body list-element-container"" data-list-length=""{length}"">
    							{nestedElement}
    							<button type=""button"" class=""btn btn-sm element-adder"">
        							<i class=""fas fa-plus""></i>
    							</button>
    						</div>
    					</div>
    				</div>";
        }

        public static string InputElementList(int index, int depth, string cssElement, string elementContent)
        {
            return $@"<!-- ELEMENT -->
    					<div data-index={index} data-depth={depth} class=""{cssElement}"">
    						<div class=""content col-xs-11"">
    							<!-- CONTENT -->
    							{elementContent}
    						</div>
    						<div class=""col-xs-1 pr-0"">
    							<button type=""button"" class=""element-deleter btn btn-sm"">
    								<i class=""fas fa-trash""></i>
    							</button>
    						</div>
    					</div>";
        }

        public static string Input(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, string inputtype, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
        {
            var control = $@"
                <div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
                	<label for=""{id}"" class=""control-label"">{labelText}</label>";
            if (inputtype == "textarea")
            {
                control += $@"<textarea rows=""10"" class=""hdm-job-input hdm-input-textarea form-control"" placeholder=""{placeholderText}"" id=""{id}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")}>{defaultValue}</textarea>";
            }
            else
            {
                control += $@"<input class=""hdm-job-input hdm-input-{inputtype} form-control"" type=""{inputtype}"" placeholder=""{placeholderText}"" id=""{id}"" value=""{defaultValue}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")} />";
            }

            if (!string.IsNullOrWhiteSpace(descriptionText))
            {
                control += $@"<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>";
            }
            control += $@"
                </div>";
            return control;
        }

        public static string InputString(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false, bool isMultiline = false)
        {
            return Input(id, cssClasses, labelText, placeholderText, descriptionText, isMultiline ? "textarea" : "text", defaultValue, isDisabled, isRequired);
        }

        public static string InputInteger(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
        {
            return Input(id, cssClasses, labelText, placeholderText, descriptionText, "number", defaultValue, isDisabled, isRequired);
        }

        public static string InputDateTime(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false, string controlConfig = "")
        {
            if (!string.IsNullOrWhiteSpace(controlConfig))
            {
                controlConfig = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(controlConfig), Formatting.None);
            }

            return $@"
            <div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
            	<label for=""{id}"" class=""control-label"">{labelText}</label>
            	<div class='hdm-job-input-container hdm-input-date-container input-group date' id='{id}_datetimepicker' data-td_options='{controlConfig}' data-td_value='{(defaultValue is DateTime dt ? dt.ToString("O") : defaultValue ?? "")}'>
            		<input type='text' class=""hdm-job-input hdm-input-date form-control"" placeholder=""{placeholderText}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")} />
            		<span class=""input-group-addon"">
            			<span class=""glyphicon glyphicon-calendar""></span>
            		</span>
            	</div>
            		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
            		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
            " : "")}
            </div>";
        }

        public static string InputBoolean(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false)
        {
            var bDefaultValue = (bool)(defaultValue ?? false);

            return $@"
            <br/>
            <div class=""form-group {cssClasses}"">
            	<div class=""form-check"">
            		<input class=""hdm-job-input hdm-input-checkbox form-check-input"" type=""checkbox"" id=""{id}"" {(bDefaultValue ? "checked='checked'" : "")} {(isDisabled ? "disabled='disabled'" : "")} />
            		<label class=""form-check-label"" for=""{id}"">{labelText}</label>
            	</div>
            		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
            		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
            " : "")}
            </div>";
        }

        public static string InputEnum(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, Dictionary<string, int> data, string defaultValue = null, bool isDisabled = false)
        {
            var initText = defaultValue != null ? defaultValue : !string.IsNullOrWhiteSpace(placeholderText) ? placeholderText : "Select a value";
            var initValue = defaultValue != null && data.ContainsKey(defaultValue) ? data[defaultValue].ToString() : "";
            var output = $@"
            <div class=""form-group {cssClasses}"">
            	<label class=""control-label"">{labelText}</label>
            	<div class=""dropdown"">
            		<button id=""{id}"" class=""hdm-job-input hdm-input-datalist btn btn-default dropdown-toggle input-control-data-list"" type=""button"" data-selectedvalue=""{initValue}"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"" {(isDisabled ? "disabled='disabled'" : "")}>
            			<span class=""{id} input-data-list-text pull-left"">{initText}</span>
            			<span class=""caret""></span>
            		</button>
            		<ul class=""dropdown-menu data-list-options"" data-optionsid=""{id}"" aria-labelledby=""{id}"">";
            foreach (var item in data)
            {
                output += $@"
            			<li><a href=""javascript:void(0)"" class=""option"" data-optiontext=""{item.Key}"" data-optionvalue=""{item.Value}"">{item.Key}</a></li>
            ";
            }

            output += $@"
            		</ul>
            	</div>
            	{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
            		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
            " : "")}
            </div>";
            return output;
        }

        public static string InputImplsMenu(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, HashSet<Type> impls, Type defaultValue = null, bool isDisabled = false, bool isRequired = false)
        {
            var initText = placeholderText ?? "Select your own implementation";
            var initValue = initText;

            if (defaultValue != null && impls.Contains(defaultValue))
            {
                var defaultType = impls.FirstOrDefault(impl => impl == defaultValue);
                initValue = defaultType.FullName;
                initText = defaultType.Name;
            }

            var output = $@"
            <div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
                <label class=""control-label"">{labelText}</label>
                <div class=""dropdown"">
                    <button id=""{id}"" class=""hdm-impl-selector-button hdm-job-input hdm-input-datalist btn btn-default dropdown-toggle input-control-data-list"" type=""button"" data-selectedvalue=""{initValue}"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"" {(isDisabled ? "disabled='disabled'" : "")}>
                        <span class=""{id} input-data-list-text pull-left"">{initText}</span>
                        <span class=""caret""></span>
                    </button>
                    <ul class=""dropdown-menu data-list-options impl-selector-options"" data-optionsid=""{id}"" aria-labelledby=""{id}"">";
                    foreach (var impl in impls)
                    {
                        var targetPanelId = $"{id}_{impl.Name}";
                        output += $@"<li><a class=""option"" data-optiontext=""{impl.Name}"" data-optionvalue=""{impl.FullName}"" data-target-panel-id=""{targetPanelId}"">{impl.Name}</a></li>";
                    }

            output += $@"</ul>
                    </div>
                {(!string.IsNullOrWhiteSpace(descriptionText) ? $@"<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>" : "")}
                </div>";

            return output;
        }
    }   
}