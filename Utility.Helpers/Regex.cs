using System.Text.RegularExpressions;

namespace Utility.Helpers
{
    public static class RegexHelper
    {
        public static string HtmlClean(this string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"\t|\n|\r|All", "").Trim();
        }

        public static bool Match(this string text, string pattern)
        {
            return Regex.Match(text, pattern).Groups.Count > 0;
        }
    }
}