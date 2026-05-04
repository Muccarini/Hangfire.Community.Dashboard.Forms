using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hangfire.Community.Dashboard.Forms.Metadata;
using Newtonsoft.Json;
using Hangfire.Community.Dashboard.Forms.Support;
using System.Collections;
using Hangfire.Community.Dashboard.Forms.Partials;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.Forms.Pages.Partials
{
	internal class JobPartial : RazorPage
	{
		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
		public readonly string JobId;
		public readonly JobMetadata Job;
		public readonly HashSet<Type> NestedTypes = new HashSet<Type>();
		public readonly List<JobHistoryMetadata> JobHistory;
		public List<object> ArgsLoaded { get; set; }
		public bool IsJobLoaded { get; set; }

		public JobPartial(string id, JobMetadata job, List<JobHistoryMetadata> jobHistory)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (job == null) throw new ArgumentNullException(nameof(job));
			JobId = id;
			Job = job;
			JobHistory = jobHistory;
		}

		public override void Execute() 
		{
			ArgsLoaded = new List<object>();
			IsJobLoaded = false;
			string jobLoadedId = Context.Request.GetQuery("jobHistoryId");

			// Check if the jobLoadedId is from the current job's history
			if (!string.IsNullOrEmpty(jobLoadedId) && JobHistory != null && JobHistory.Any(his => his.Id == jobLoadedId))
			{
				JobsHistoryHelper.GetJobArguments(jobLoadedId, Context).ToList()
					.ForEach(arg => ArgsLoaded.Add(arg));
				IsJobLoaded = true;
			}

			var inputs = string.Empty;
			var parameters = Job.MethodInfo.GetParameters();

			for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
			{
				var parameterInfo = parameters[parameterIndex];
				if (!Attribute.IsDefined(parameterInfo, typeof(DisplayDataAttribute)))
					continue;

				DisplayDataAttribute displayInfo = parameterInfo.GetCustomAttribute<DisplayDataAttribute>();
				displayInfo.Label = displayInfo.Label ?? parameterInfo.Name;
				var defaultValue = IsJobLoaded && parameterIndex < ArgsLoaded.Count 
					? ArgsLoaded[parameterIndex] 
					: displayInfo?.DefaultValue;
				var id = $"{JobId}_{parameterInfo.Name}";

				inputs += TypePartial.ToHtml(parameterInfo.ParameterType, id, displayInfo, 0, defaultValue);
			}

			var hasInputs = !string.IsNullOrWhiteSpace(inputs);
			var formContent = hasInputs 
				? inputs 
				: $@"<div class=""hdm-no-inputs"" role=""status"">
					<span class=""glyphicon glyphicon-ok-circle hdm-no-inputs-icon"" aria-hidden=""true""></span>
					<span class=""hdm-no-inputs-text"">This job does not require any input parameters.</span>
				</div>";

			var formId = $"form_{JobId}";
			var errorId = $"{JobId}_error";
			var successId = $"{JobId}_success";

			WriteLiteral($@"
				<fieldset class=""hdm-job-fieldset"" id=""{formId}"">
					<legend class=""sr-only"">Job Parameters for {System.Net.WebUtility.HtmlEncode(Job.Name)}</legend>
					<div class=""well hdm-job-inputs-container"" role=""group"" aria-label=""Job input parameters"">
						{formContent}
					</div>
				</fieldset>
				<div id=""{errorId}"" class=""hdm-job-error"" role=""alert"" aria-live=""assertive"" aria-atomic=""true""></div>
				<div id=""{successId}"" class=""hdm-job-success"" role=""status"" aria-live=""polite"" aria-atomic=""true""></div>
			");
		}

	}
}
