using Hangfire;
using Hangfire.Community.Dashboard.Forms;
using Hangfire.Community.Dashboard.Forms.Support;
using Hangfire.MemoryStorage;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: Microsoft.Owin.OwinStartup(typeof(ASP.Net_Web_Application.Startup))]

namespace ASP.Net_Web_Application
{
	public class Startup
	{
		private IEnumerable<IDisposable> GetHangfireServers()
		{
			GlobalConfiguration.Configuration
				.UseMemoryStorage()
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseManagementPages(typeof(Startup).Assembly);

			var options = new BackgroundJobServerOptions();
			var queues = new List<string>();
			queues.Add("default");
			queues.AddRange(JobsHelper.GetAllQueues());

			options.Queues = queues.Distinct().ToArray();
			yield return new BackgroundJobServer(options);
		}

		public void Configuration(IAppBuilder app)
		{
			app.UseHangfireAspNet(GetHangfireServers);
			app.UseHangfireDashboard("/hangfire", new DashboardOptions() {
				DisplayStorageConnectionString = false,
				DashboardTitle = "ASP.Net Hangfire Forms",
				StatsPollingInterval = 5000
			});
		}
	}
}
