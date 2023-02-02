using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility.Helpers
{
    public static class StringHelper
    {
        public static string Join(this string value, string seperator) => string.Join(seperator, value);

        public static string Join(this string value, char seperator) => string.Join(seperator, value);

        public static string Join(this IEnumerable<string> value, string seperator) => string.Join(seperator, value);

        public static string Join(this IEnumerable<string> value, char seperator) => string.Join(seperator, value);

        public static Stream ToStream(this string[] str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            foreach (var s in str) writer.WriteLine(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        //https://stackoverflow.com/questions/3947126/case-insensitive-list-search
        //Adam Sills
        public static bool Contains(this IEnumerable<string> source, string keyword, StringComparison comp)
        {
            return source.Any(s => s.Equals(keyword, StringComparison.OrdinalIgnoreCase));
        }

        // JaredPar
        //https://stackoverflow.com/questions/444798/case-insensitive-containsstring
        //string title = "STRING";
        //bool contains = title.Contains("string", StringComparison.OrdinalIgnoreCase);
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string RemoveLineEndings(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty).Replace("\t", String.Empty);
        }

        public static String ReduceWhitespace(this String value)
        {
            var newString = new StringBuilder();
            bool previousIsWhitespace = false;
            for (int i = 0; i < value.Length; i++)
            {
                if (Char.IsWhiteSpace(value[i]))
                {
                    if (previousIsWhitespace)
                    {
                        continue;
                    }

                    previousIsWhitespace = true;
                }
                else
                {
                    previousIsWhitespace = false;
                }

                newString.Append(value[i]);
            }

            return newString.ToString();
        }

        public static int ToNumber(this string str)
        {
            return Encoding.ASCII.GetBytes(str).Select(a => (int)a).Sum();
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

        public static bool IsAllLetters(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        }

        // Only Numbers:

        public static bool IsAllDigits(string s)
        {
            foreach (char c in s)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }

        //  Only Numbers Or Letters:

        public static bool IsAllLettersOrDigits(string s)
        {
            foreach (char c in s)
            {
                if (!Char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }

        // Only Numbers Or Letters Or Underscores:

        public static bool IsAllLettersOrDigitsOrUnderscores(string s)
        {
            foreach (char c in s)
            {
                if (!Char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }
            return true;
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

        /// <summary>
        /// Use Humanizer library
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Pluralise(this string source, int count)
        {
            if (count == 1) return $"{count} {source}";
            return $"{count} {source}s"; ;
        }

        public static string ToDelimited<T>(this IEnumerable<T> source, string delimiter = ",") where T : notnull
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return string.Join(delimiter, source.WithDelimiter(delimiter));
        }

        public static IEnumerable<string> WithDelimiter<T>(this IEnumerable<T> values, string delimiter) where T : notnull
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (!values.Any())
                yield return string.Empty;

            yield return values.Select(t => t.ToString()).First();

            foreach (var item in values.Skip(1))
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

        public static void SetPropertyByType<T>(object obj, T value)
        {
            var properties = obj.GetType().GetProperties();
            var prop = properties.SingleOrDefault(a => a.PropertyType == typeof(T));
            prop.SetValue(obj, value, null);
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
            return (double)parsedD;
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
            System.Uri uriResult;
            return System.Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == System.Uri.UriSchemeHttp || uriResult.Scheme == System.Uri.UriSchemeHttps);
        }

        //public static string UrlToDomain(this string str)
        //{
        //    DomainName completeDomain;
        //    return DomainName.TryParse(new Uri(str).Host, out completeDomain) ? completeDomain.SLD : "";
        //}

        private static readonly char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };

        public static string ToLineSeparatedString(this IEnumerable<KeyValuePair<string, string>> keyValues)
            => keyValues.Select(keyValuePair => keyValuePair.Key + ": " + keyValuePair.Value)
                .ToLineSeparatedString();

        public static string ToLineSeparatedString(this IEnumerable<string> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var value in values)
            {
                stringBuilder.AppendLine(value);
            }
            return stringBuilder.ToString();
        }

        public static string Concatenate(this IEnumerable<string> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var value in values)
            {
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            return ReplaceAtPosition(text, search, replace, text.IndexOf(search, StringComparison.Ordinal));
        }

        public static string ReplaceLast(this string text, string search, string replace)
        {
            return ReplaceAtPosition(text, search, replace, text.LastIndexOf(search, StringComparison.Ordinal));
        }

        /// <summary>
        /// Replaces <see cref="search"/> with <see cref="replace"/> at <see cref="index"/>
        /// if <see cref="text"/> contains <see cref="search"/> at <see cref="index"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ReplaceAtPosition(this string text, string search, string replace, int index) =>

            index >= 0 && search == text.Substring(index, search.Length) ?
                    text.Substring(0, index) + replace + text.Substring(index + search.Length)
                    : text;

        /// <summary>
        /// Removes <see cref="search"/> at <see cref="index"/>
        /// if <see cref="text"/> contains <see cref="search"/> at <see cref="index"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string RemoveAtPosition(this string text, string search, int index) => ReplaceAtPosition(text, search, String.Empty, index);

        /// <summary>
        /// Remove the digits from start of input.
        /// </summary>
        /// <example>
        /// 122 km -> km
        /// </example>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveDigitsFromStart(this string input) => input.TrimStart(Digits);

        public static string TakeDigits(this string input) => string.Concat(input.Join(Digits, a => a, a => a, (a, b) => a == b ? a : char.MinValue));

        [Pure]
        public static string[] SplitLines(this string input)
        {
            var list = new List<string>();
            using (var reader = new StringReader(input))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    list.Add(str);
                }
            }

            return list.ToArray();
        }

        [Pure]
        public static string Replace(this string source, string oldValue, string newValue, StringComparison comparisonType)
        {
            // from http://stackoverflow.com/a/22565605 with some adaptions
            if (string.IsNullOrEmpty(oldValue))
            {
                throw new ArgumentNullException(nameof(oldValue));
            }

            if (source.Length == 0)
            {
                return source;
            }

            if (newValue == null)
            {
                newValue = string.Empty;
            }

            var result = new StringBuilder();
            int startingPos = 0;
            int nextMatch;
            while ((nextMatch = source.IndexOf(oldValue, startingPos, comparisonType)) > -1)
            {
                result.Append(source, startingPos, nextMatch - startingPos);
                result.Append(newValue);
                startingPos = nextMatch + oldValue.Length;
            }

            result.Append(source, startingPos, source.Length - startingPos);

            return result.ToString();
        }

        [Pure]
        public static string TrimSlashes(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (trimmed.Length == 2
                && char.IsLetter(trimmed[0])
                && trimmed[1] == ':')
            {
                return trimmed + Path.DirectorySeparatorChar;
            }

            return trimmed;
        }
    }
}