using System;

namespace Hangfire.Community.Dashboard.Forms.Metadata
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ShowMetaDataAttribute : Attribute
	{
		/// <summary>
		/// Controls if the Metadata for the job is displayed.
		/// </summary>
		public bool ShowOnUI { get; set; } = false;

		public ShowMetaDataAttribute()
		{ }

		public ShowMetaDataAttribute(bool show)
		{
			ShowOnUI = show;
		}
	}
}
