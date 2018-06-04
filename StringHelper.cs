using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class StringHelper
    {


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
    }
}
