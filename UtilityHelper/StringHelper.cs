using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    public static class StringHelper
    {
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

        public static string Concatenate(this IEnumerable<char> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var value in values)
            {
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        public static string Concatenate(params char[] values)
        {
            return values.Concatenate();
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
    }
}
