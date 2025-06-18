﻿using Hangfire;
using Hangfire.Dashboard.Management.v3.Metadata;
using Hangfire.Dashboard.Management.v3.Support;
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