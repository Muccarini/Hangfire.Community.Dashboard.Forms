using Hangfire;
using Hangfire.Dashboard.Management.Dynamic.Metadata;
using Hangfire.Dashboard.Management.Dynamic.Support;
using Hangfire.Server;

namespace ASP.Net_Core_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Simple Implementation", Title = nameof(Simple))]
	public class Simple : IJob
	{
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}
}
