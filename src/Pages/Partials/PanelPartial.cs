using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Community.Dashboard.Forms.Metadata;
using Hangfire.Community.Dashboard.Forms.Support;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Newtonsoft.Json;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.Forms.Pages.Partials
{
	partial class PanelPartial
	{
		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
		public readonly string SectionName;
		public readonly List<JobMetadata> Jobs;
		public readonly Dictionary<string, List<JobHistoryMetadata>> JobsHistory = new Dictionary<string, List<JobHistoryMetadata>>();

		public PanelPartial(string section, List<JobMetadata> jobs)
		{
			if (section == null) throw new ArgumentNullException(nameof(section));
			if (jobs == null) throw new ArgumentNullException(nameof(jobs));
			SectionName = section;
			Jobs = jobs;

			foreach (var job in jobs)
			{
				JobsHistory[$"{job.MethodName}"] = new List<JobHistoryMetadata>();

				//Context is still null here, we must wait for dispatcher to set it on Execute()
				var jobHistory = JobsHistoryHelper.GetJobHistoryByName($"{job.MethodName}", 10, job.Queue, Context);

				if (jobHistory != null && jobHistory.Count > 0)
				{
					JobsHistory[$"{job.MethodName}"] = jobHistory;
				}
			}

		}

	}

}
