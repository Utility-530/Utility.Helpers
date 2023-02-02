using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Helpers.NonGeneric
{
    public static class Filter
    {
        public static IEnumerable FilterByIndices(this IEnumerable enumerable, IEnumerable<int> indices, bool includenull = false)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (indices.Contains(i))
                    yield return enumerator.Current;
                else
                    if (includenull)
                    yield return null;
                i++;
            }
        }

        public static IEnumerable FilterWithNull<R>(this IEnumerable data, params KeyValuePair<string, R>[] kvps) where R : class, IConvertible
        {
            return data.FilterByIndices(FilterIndex(data, kvps.Select(a => a.Key), kvps.Select(a => a.Value)), true);
        }

        public static IEnumerable FilterDefault<R>(this IEnumerable data, params KeyValuePair<string, R>[] kvps) where R : class, IConvertible
        {
            return data.FilterByIndices(FilterIndex(data, kvps.Select(a => a.Key), kvps.Select(a => a.Value)));
        }

        public static IEnumerable<int> FilterIndex<R>(this IEnumerable data, IEnumerable<string> filter, IEnumerable<R> filterOn) where R : class, IConvertible
        {
            IEnumerator<string> fenm = (filter).GetEnumerator();
            IEnumerator<R> fenom = (filterOn).GetEnumerator();

            var filtered = new int[] { };

            while (fenm.MoveNext() && fenom.MoveNext())
            {
                var filted = data.GetPropertyRefValues<R>(fenm.Current).ToList();
                filtered = Generic.Filter.Select(filted.Where(a => a != null).OfType<R>(), fenom.Current).Union(filtered).ToArray();
            }

            return filtered;
        }
    }
}