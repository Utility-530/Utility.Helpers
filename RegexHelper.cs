using System;
using System.Collections.Generic;
using System.Text;

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
