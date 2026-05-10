using System.Linq;
using System.Text.RegularExpressions;

namespace Hangfire.Community.Dashboard.Forms.Support
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Converts a string into a valid HTML element ID by replacing invalid characters.
		/// Ensures the ID follows HTML5 specification (letters, digits, hyphens, underscores only).
		/// </summary>
		/// <param name="str">The string to sanitize</param>
		/// <returns>A valid HTML element ID</returns>
		public static string SanitizeHtmlId(this string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			// Replace spaces and invalid ID characters with underscores
			// Valid HTML IDs: letters, digits, hyphens, underscores, colons, periods
			// We'll be conservative and use only letters, digits, hyphens, and underscores
			var scrubbed = Regex.Replace(str, @"[^a-zA-Z0-9_-]", "_");
			
			// Ensure it doesn't start with a digit, hyphen, or multiple underscores
			scrubbed = Regex.Replace(scrubbed, @"^[0-9-]+", "");
			scrubbed = Regex.Replace(scrubbed, @"_{2,}", "_");
			scrubbed = scrubbed.Trim('_');
			
			return string.IsNullOrEmpty(scrubbed) ? "id" : scrubbed;
		}
	}
}
