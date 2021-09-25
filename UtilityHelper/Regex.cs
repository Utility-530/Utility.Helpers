namespace UtilityHelper
{
    public class RegexHelper
    {
        public static string HtmlClean(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"\t|\n|\r|All", "").Trim();
        }
    }
}