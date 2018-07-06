using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class StringHelper
    {



        public static int ToNumber(this string str)
        {
            return  Encoding.ASCII.GetBytes(str).Select(_=>(int)_).Sum();

        }


        public static bool IsDigitsOnly(this string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }


        public static bool HasAnyDigits(this string str)
        {
            foreach (char c in str)
            {
                if (c >= '0' & c <= '9')
                    return true;
            }

            return false;
        }


        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool Parse(string s, string format, out DateTime dt)
        {
            return DateTime.TryParseExact(
                s,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt
            );
        }


        public static string Pluralise(this string source, int count)
        {
            if (count == 1) return $"{count} {source}";
            return $"{count} {source}s"; ;
        }

        public static string ToDelimited<T>(this IEnumerable<T> source, string delimiter = ",")
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return string.Join(delimiter, source.WithDelimiter(delimiter));

        }

        public static IEnumerable<string> WithDelimiter<T>(this IEnumerable<T> source, string delimiter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var array = source;//.AsArray();
            if (!array.Any()) yield return string.Empty;

            yield return array.Select(t => t.ToString()).First();

            foreach (var item in array.Skip(1))
                yield return $"{delimiter}{item}";

        }



           public static bool HasValueBetween(this string str, string start, string end)
        {
            return !string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end) &&
                   str.Contains(start) &&
                   str.Contains(end) &&
                   str.IndexOf(start, StringComparison.Ordinal) < str.IndexOf(end, StringComparison.Ordinal);
        }

        public static string Between(this string str, string start, string end)
        {
            return Regex.Match(str, $@"\{start}([^)]*)\{end}").Groups[1].Value;
        }

        public static string UntilWithout(this string str, string end)
        {
            return str.Split(new[] { end }, StringSplitOptions.None)[0];
        }

        public static string RemoveHTMLSymbols(this string str)
        {
            return str.Replace("&nbsp;", "")
                .Replace("&amp;", "");
        }

        public static bool IsNullWhiteSpaceOrDefault(this string str, string defVal)
        {
            return string.IsNullOrWhiteSpace(str) || str == defVal;
        }

        public static bool ContainsAny(this string str, params string[] strings)
        {
            return strings.Any(str.Contains);
        }

        public static string Remove(this string str, string substring)
        {
            return str.Replace(substring, "");
        }

        public static string RemoveMany(this string str, params string[] substrings)
        {
            return substrings.Aggregate(str, (current, substring) => current.Remove(substring));
        }

        public static bool IsSimilarTo(this string str, string otherStr)
        {
            var m = new Metaphone();
            return m.Encode(str) == m.Encode(otherStr);
        }

        public static bool IsSimilarToAny(this string str, params string[] strings)
        {
            return strings.Any(s => s.IsSimilarTo(str));
        }

        public static string[] Split(this string str, string separator, bool includeSeparator = false)
        {
            var split = str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            if (includeSeparator)
            {
                var splitWithSeparator = new string[split.Length + split.Length - 1];
                var j = 0;
                for (var i = 0; i < splitWithSeparator.Length; i++)
                {
                    if (i % 2 == 1)
                        splitWithSeparator[i] = separator;
                    else
                        splitWithSeparator[i] = split[j++];
                }
                split = splitWithSeparator;
            }
            return split;
        }

        public static string[] SplitByFirst(this string str, params string[] strings)
        {
            foreach (var s in strings)
                if (str.Contains(s))
                    return str.Split(s);
            return new[] { str };
        }
        
        public static string[] SameWords(this string str, string otherStr, bool casaeSensitive = false, string splitBy = " ", int minWordLength = 1)
        {
            if (casaeSensitive)
            {
                str = str.ToLower();
                otherStr = otherStr.ToLower();
            }
            
            var str1Arr = str.Split(splitBy);
            var str2Arr = otherStr.Split(splitBy);
            var intersection = str1Arr.Intersect(str2Arr).Where(w => w.Length >= minWordLength);
            return intersection.ToArray();
        }

        public static string[] SameWords(this string str, string[] otherStrings, bool casaeSensitive, string splitBy = " ", int minWordLength = 1)
        {
            var sameWords = new List<string>();

            foreach (var otherStr in otherStrings)
                sameWords.AddRange(str.SameWords(otherStr, casaeSensitive, splitBy, minWordLength));

            return sameWords.Distinct().ToArray();
        }

        public static string[] SameWords(this string str, params string[] otherStrings)
        {
            return str.SameWords(otherStrings, false, " ", 1);
        }

        public static bool HasSameWords(this string str, string otherStr, bool caseSensitive = false, string splitBy = " ", int minWordLength = 1)
        {
            return str.SameWords(otherStr, caseSensitive, splitBy, minWordLength).Any();
        }

        public static bool HasSameWords(this string str, string[] otherStrings, bool caseSensitive, string splitBy = " ", int minWordLength = 1)
        {
            return str.SameWords(otherStrings, caseSensitive, splitBy, minWordLength).Any();
        }

        public static bool HasSameWords(this string str, params string[] otherStrings)
        {
            return str.SameWords(otherStrings, false, " ", 1).Any();
        }

      

        public static double? TryToDouble(this string str)
        {
            str = str.Replace(',', '.');
            double value;
            var isParsable = double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            if (isParsable)
                return value;
            return null;
        }

        public static double ToDouble(this string str)
        {
            var parsedD = str.TryToDouble();
            if (parsedD == null)
                throw new InvalidCastException("Nie można sparsować wartości double");
            return (double) parsedD;
        }

        public static bool IsDouble(this string str)
        {
            return str.TryToDouble() != null;
        }

        public static bool StartsWithAny(this string str, params string[] strings)
        {
            return strings.Any(str.StartsWith);
        }

        public static bool EndsWithAny(this string str, params string[] strings)
        {
            return strings.Any(str.EndsWith);
        }

        public static bool ContainsAll(this string str, params string[] strings)
        {
            return strings.All(str.Contains);
        }

        public static string RemoveWord(this string str, string word, string separator = " ")
        {
            return string.Join(separator, str.Split(separator).Where(w => w != word));
        }

        public static string RemoveWords(this string str, string[] words, string separator)
        {
            foreach (var w in words)
                str = str.RemoveWord(w);
            return str;
        }

        public static string RemoveWords(this string str, params string[] words)
        {
            return str.RemoveWords(words, " ");
        }

        public static bool IsUrl(this string str)
        {
            Uri uriResult;
            return Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        //public static string UrlToDomain(this string str)
        //{
        //    DomainName completeDomain;
        //    return DomainName.TryParse(new Uri(str).Host, out completeDomain) ? completeDomain.SLD : "";
        //}
    }



}
