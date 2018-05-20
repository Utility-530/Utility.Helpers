using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// From Roland Pheasant's Dynamic Trader App
//https://github.com/RolandPheasant/Dynamic.Trader

namespace UtilityHelper
{

    public static class Delimitation
    {

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
