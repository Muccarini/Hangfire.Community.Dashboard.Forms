using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.Community.Dashboard.Forms.Metadata
{

	[AttributeUsage(AttributeTargets.Method)]
	public class IgnoreMethodAttribute : Attribute
	{
		public IgnoreMethodAttribute()
		{

		}
	}
}
