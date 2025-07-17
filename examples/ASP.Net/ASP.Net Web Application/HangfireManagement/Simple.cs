using Hangfire;
using Hangfire.Community.Dashboard.Forms.Metadata;
using Hangfire.Community.Dashboard.Forms.Support;
using Hangfire.Server;

namespace ASP.Net_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Simple Implementation", Title = nameof(Simple))]
	public class Simple : IJob
	{
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}
}