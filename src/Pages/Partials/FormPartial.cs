using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Community.Dashboard.Forms.Support;

namespace Hangfire.Community.Dashboard.Forms.Pages.Partials
{
    public static class FormPartial
    {
        public static string InputList(string myId, string labelText, int length, string nestedElement)
        {
            return $@"
                <div class=""panel panel-default hdm-list-panel"">
                    <div class=""panel-heading hdm-list-header"" role=""button"" tabindex=""0"" data-toggle=""collapse"" href=""#collapse_{myId}"" aria-expanded=""false"" aria-controls=""collapse_{myId}"">
                        <h4 class=""panel-title hdm-list-title"">
                            <span class=""glyphicon glyphicon-list hdm-list-icon"" aria-hidden=""true""></span>
                            {labelText}
                            <span class=""hdm-list-count-badge"">{length} items</span>
                        </h4>
                    </div>
                    <div id=""collapse_{myId}"" class=""panel-collapse collapse"" role=""region"" aria-labelledby=""heading_{myId}"">
                        <div id=""{myId}"" class=""panel-body list-element-container hdm-list-container"" data-list-length=""{length}"" role=""list"" aria-label=""{labelText} items"">
                            {nestedElement}
                            <button type=""button"" class=""btn btn-sm element-adder hdm-add-item-btn"" aria-label=""Add new item to {labelText}"">
                                <i class=""fas fa-plus"" aria-hidden=""true""></i>
                                <span class=""sr-only"">Add item</span>
                            </button>
                        </div>
                    </div>
                </div>";
        }

        public static string InputElementList(int index, int depth, string cssElement, string elementContent)
        {
            return $@"
                <div data-index=""{index}"" data-depth=""{depth}"" class=""{cssElement} hdm-list-item"" role=""listitem"">
                    <div class=""content col-xs-11 hdm-item-content"">
                        {elementContent}
                    </div>
                    <div class=""col-xs-1 pr-0 hdm-item-actions"">
                        <button type=""button"" class=""element-deleter btn btn-sm hdm-delete-item-btn"" aria-label=""Remove this item"">
                            <i class=""fas fa-trash"" aria-hidden=""true""></i>
                            <span class=""sr-only"">Remove</span>
                        </button>
                    </div>
                </div>";
        }

        public static string Input(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, string inputtype, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
        {
            var requiredAttr = isRequired ? "required=\"required\" aria-required=\"true\"" : "";
            var disabledAttr = isDisabled ? "disabled=\"disabled\" aria-disabled=\"true\"" : "";
            var describedBy = !string.IsNullOrWhiteSpace(descriptionText) ? $"aria-describedby=\"{id}Help\"" : "";
            
            var control = $@"
                <div class=""form-group hdm-form-group {cssClasses} {(isRequired ? "required" : "")}"">
                    <label for=""{id}"" class=""control-label hdm-label"">{labelText}</label>";
            
            if (inputtype == "textarea")
            {
                control += $@"
                    <textarea rows=""10"" class=""hdm-job-input hdm-input-textarea form-control"" placeholder=""{placeholderText}"" id=""{id}"" {disabledAttr} {requiredAttr} {describedBy}>{defaultValue}</textarea>";
            }
            else
            {
                control += $@"
                    <input class=""hdm-job-input hdm-input-{inputtype} form-control"" type=""{inputtype}"" placeholder=""{placeholderText}"" id=""{id}"" value=""{defaultValue}"" {disabledAttr} {requiredAttr} {describedBy} />";
            }

            if (!string.IsNullOrWhiteSpace(descriptionText))
            {
                control += $@"
                    <small id=""{id}Help"" class=""form-text text-muted hdm-help-text"">{descriptionText}</small>";
            }
            control += @"
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
            string tdOptionsAttr = "";
            if (!string.IsNullOrWhiteSpace(controlConfig))
            {
                try
                {
                    var deserializedConfig = JsonConvert.DeserializeObject(controlConfig);
                    var serializedConfig = JsonConvert.SerializeObject(deserializedConfig, Formatting.None);
                    var encodedConfig = System.Net.WebUtility.HtmlEncode(serializedConfig);
                    tdOptionsAttr = $"data-td_options=\"{encodedConfig}\"";
                }
                catch
                {
                    // If deserialization fails, don't add the attribute
                    tdOptionsAttr = "";
                }
            }

            var requiredAttr = isRequired ? "required=\"required\" aria-required=\"true\"" : "";
            var disabledAttr = isDisabled ? "disabled=\"disabled\" aria-disabled=\"true\"" : "";
            var describedBy = !string.IsNullOrWhiteSpace(descriptionText) ? $"aria-describedby=\"{id}Help\"" : "";

            return $@"
            <div class=""form-group hdm-form-group {cssClasses} {(isRequired ? "required" : "")}"">
                <label for=""{id}_input"" class=""control-label hdm-label"">{labelText}</label>
                <div class=""hdm-job-input-container hdm-input-date-container input-group date"" id=""{id}_datetimepicker"" {tdOptionsAttr} data-td_value=""{(defaultValue is DateTime dt ? dt.ToString("O") : defaultValue ?? "")}"">
                    <input type=""text"" id=""{id}_input"" class=""hdm-job-input hdm-input-date form-control"" placeholder=""{placeholderText}"" {disabledAttr} {requiredAttr} {describedBy} />
                    <span class=""input-group-addon"" aria-hidden=""true"">
                        <span class=""glyphicon glyphicon-calendar""></span>
                    </span>
                </div>
                {(!string.IsNullOrWhiteSpace(descriptionText) ? $@"<small id=""{id}Help"" class=""form-text text-muted hdm-help-text"">{descriptionText}</small>" : "")}
            </div>";
        }

        public static string InputBoolean(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false)
        {
            var bDefaultValue = (bool)(defaultValue ?? false);
            var disabledAttr = isDisabled ? "disabled=\"disabled\" aria-disabled=\"true\"" : "";
            var describedBy = !string.IsNullOrWhiteSpace(descriptionText) ? $"aria-describedby=\"{id}Help\"" : "";

            return $@"
            <div class=""form-group hdm-form-group hdm-checkbox-group {cssClasses}"">
                <div class=""form-check hdm-form-check"">
                    <input class=""hdm-job-input hdm-input-checkbox form-check-input"" type=""checkbox"" id=""{id}"" {(bDefaultValue ? "checked=\"checked\"" : "")} {disabledAttr} {describedBy} />
                    <label class=""form-check-label hdm-checkbox-label"" for=""{id}"">{labelText}</label>
                </div>
                {(!string.IsNullOrWhiteSpace(descriptionText) ? $@"<small id=""{id}Help"" class=""form-text text-muted hdm-help-text"">{descriptionText}</small>" : "")}
            </div>";
        }

        public static string InputEnum(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, Dictionary<string, int> data, string defaultValue = null, bool isDisabled = false)
        {
            var initText = defaultValue != null ? defaultValue : !string.IsNullOrWhiteSpace(placeholderText) ? placeholderText : "Select a value";
            var initValue = defaultValue != null && data.ContainsKey(defaultValue) ? data[defaultValue].ToString() : "";
            var disabledAttr = isDisabled ? "disabled=\"disabled\" aria-disabled=\"true\"" : "";
            var describedBy = !string.IsNullOrWhiteSpace(descriptionText) ? $"aria-describedby=\"{id}Help\"" : "";
            
            var output = $@"
            <div class=""form-group hdm-form-group hdm-dropdown-group {cssClasses}"">
                <label class=""control-label hdm-label"" id=""{id}_label"">{labelText}</label>
                <div class=""dropdown hdm-dropdown"">
                    <button id=""{id}"" class=""hdm-job-input hdm-input-datalist btn btn-default dropdown-toggle input-control-data-list hdm-dropdown-btn"" type=""button"" data-selectedvalue=""{initValue}"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"" aria-labelledby=""{id}_label"" {disabledAttr} {describedBy}>
                        <span class=""{id} input-data-list-text pull-left hdm-dropdown-text"">{initText}</span>
                        <span class=""caret"" aria-hidden=""true""></span>
                    </button>
                    <ul class=""dropdown-menu data-list-options hdm-dropdown-menu"" data-optionsid=""{id}"" aria-labelledby=""{id}"" role=""listbox"">";
            
            foreach (var item in data)
            {
                output += $@"
                        <li role=""option"">
                            <a href=""javascript:void(0)"" class=""option hdm-dropdown-option"" data-optiontext=""{item.Key}"" data-optionvalue=""{item.Value}"">{item.Key}</a>
                        </li>";
            }

            output += $@"
                    </ul>
                </div>
                {(!string.IsNullOrWhiteSpace(descriptionText) ? $@"<small id=""{id}Help"" class=""form-text text-muted hdm-help-text"">{descriptionText}</small>" : "")}
            </div>";
            return output;
        }

        public static string InputImplsMenu(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, HashSet<Type> impls, Type defaultValue = null, bool isDisabled = false, bool isRequired = false)
        {
            var initText = placeholderText ?? "Select your own implementation";
            var initValue = "";

            if (defaultValue != null && impls.Contains(defaultValue))
            {
                var defaultType = impls.FirstOrDefault(impl => impl == defaultValue);
                initValue = defaultType.FullName;
                initText = VT.GetDisplayName(defaultType);
            }

            var disabledAttr = isDisabled ? "disabled=\"disabled\" aria-disabled=\"true\"" : "";
            var requiredAttr = isRequired ? "aria-required=\"true\"" : "";
            var describedBy = !string.IsNullOrWhiteSpace(descriptionText) ? $"aria-describedby=\"{id}Help\"" : "";

            var output = $@"
            <div class=""form-group hdm-form-group hdm-impl-selector-group {cssClasses} {(isRequired ? "required" : "")}"">
                <label class=""control-label hdm-label"" id=""{id}_label"">{labelText}</label>
                <div class=""dropdown hdm-dropdown"">
                    <button id=""{id}"" class=""hdm-impl-selector-button hdm-job-input hdm-input-datalist btn btn-default dropdown-toggle input-control-data-list hdm-dropdown-btn"" type=""button"" data-selectedvalue=""{initValue}"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"" aria-labelledby=""{id}_label"" {disabledAttr} {requiredAttr} {describedBy}>
                        <span class=""{id} input-data-list-text pull-left hdm-dropdown-text"">{initText}</span>
                        <span class=""caret"" aria-hidden=""true""></span>
                    </button>
                    <ul class=""dropdown-menu data-list-options impl-selector-options hdm-dropdown-menu"" data-optionsid=""{id}"" aria-labelledby=""{id}"" role=""listbox"">";
            
            foreach (var impl in impls)
            {
                var targetPanelId = $"{id}_{impl.Name}";
                var displayName = VT.GetDisplayName(impl);
                output += $@"
                        <li role=""option"">
                            <a class=""option hdm-dropdown-option"" data-optiontext=""{displayName}"" data-optionvalue=""{impl.FullName}"" data-target-panel-id=""{targetPanelId}"" href=""javascript:void(0)"">{displayName}</a>
                        </li>";
            }

            output += $@"
                    </ul>
                </div>
                {(!string.IsNullOrWhiteSpace(descriptionText) ? $@"<small id=""{id}Help"" class=""form-text text-muted hdm-help-text"">{descriptionText}</small>" : "")}
            </div>";

            return output;
        }
    }   
}
