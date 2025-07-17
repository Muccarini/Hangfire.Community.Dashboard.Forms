using System.Linq;

namespace Hangfire.Community.Dashboard.Forms.Support
{
	public static class ExtensionMethods
	{
		public static string ScrubURL(this string seed) => System.Web.HttpUtility.HtmlEncode(seed);
	}
}
