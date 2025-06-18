using Hangfire;
using Hangfire.Dashboard.Management.v3.Metadata;
using Hangfire.Dashboard.Management.v3.Support;
using Hangfire.Server;

namespace ASP.Net_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Multiple Classes in One Menu", Title = nameof(Simple2))]
	public class Simple2 : IJob
	{
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}

	[ManagementPage(MenuName = "Multiple Classes in One Menu", Title = nameof(Simple3))]
	public class Simple3 : IJob
	{
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}
}