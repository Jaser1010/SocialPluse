using System.Text.RegularExpressions;

namespace SocialPluse.Services.Extensions
{
	public static class SanitizationExtensions
	{
		// Simple regex-based HTML stripping for a clean graduation project baseline
		public static string Sanitize(this string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return text;
			// Strips all HTML tags to prevent XSS
			return Regex.Replace(text, "<.*?>", string.Empty).Trim();
		}
	}
}