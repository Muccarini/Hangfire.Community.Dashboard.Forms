using System;
using System.Collections.Generic;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.Forms.Support
{
	public static class ManagementSidebarItemCollection
	{
		public static List<Func<RazorPage, MenuItem>> Items = new List<Func<RazorPage, MenuItem>>();
	}
}
