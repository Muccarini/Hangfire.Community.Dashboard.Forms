using System;
using System.Collections.Generic;
using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.Forms.Pages.Partials
{
	partial class ManagementSidebarPartial
	{
		public ManagementSidebarPartial([NotNull] IEnumerable<Func<RazorPage, MenuItem>> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			Items = items;
		}

		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
	}
}
