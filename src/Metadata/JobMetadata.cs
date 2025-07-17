﻿using System;
using System.Reflection;
using Hangfire.Community.Dashboard.Forms.Support;

namespace Hangfire.Community.Dashboard.Forms.Metadata
{
	public class JobMetadata
	{
		public string SectionTitle { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string MenuName { get; set; }
		public bool AllowMultiple { get; set; }

		public string Queue { get; set; }
		public Type Type { get; set; }
		public MethodInfo MethodInfo { get; set; }

		public string MethodName => Type.Name + "_" + MethodInfo.Name;
		public string JobId => $"{MenuName}/{MethodName.ScrubURL()}";
		public string Name => $"{DisplayName ?? MethodName}";
	}
}
