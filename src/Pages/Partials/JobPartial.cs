using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hangfire.Dashboard.Management.Dynamic.Metadata;
using Newtonsoft.Json;
using Hangfire.Dashboard.Management.Dynamic.Support;
using System.Collections;
using Hangfire.Dashboard.Management.Dynamic.Partials;

namespace Hangfire.Dashboard.Management.Dynamic.Pages.Partials
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
			int parameterIndex = 1;

			foreach (var parameterInfo in Job.MethodInfo.GetParameters()
			.Where(par => Attribute.IsDefined(par, typeof(DisplayDataAttribute))))
			{
				parameterIndex++;
				DisplayDataAttribute displayInfo = parameterInfo.GetCustomAttribute<DisplayDataAttribute>();
				displayInfo.Label = displayInfo.Label ?? parameterInfo.Name;
				var defaultValue = IsJobLoaded ? ArgsLoaded[parameterIndex] : displayInfo?.DefaultValue;
				var id = $"{JobId}_{parameterInfo.Name}";

				inputs += TypePartial.ToHtml(parameterInfo.ParameterType, id, displayInfo, 0, defaultValue);
			}

			if (string.IsNullOrWhiteSpace(inputs))
			{
				inputs = "<span>This job does not require inputs</span>";
			}

			WriteLiteral($@"
				<div class=""well"">
					{inputs}
				</div>
				<div id=""{JobId}_error""></div>
				<div id=""{JobId}_success""></div>
			");
		}

	}
}
