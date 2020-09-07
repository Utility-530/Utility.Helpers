using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper.NonGeneric
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

        public static IEnumerable FilterWithNull<R>(this IEnumerable data, params KeyValuePair<string, R>[] kvps) where R : IConvertible
        {
            return data.FilterByIndices(FilterIndex(data, kvps.Select(_ => _.Key), kvps.Select(_ => _.Value)), true);
        }

        public static IEnumerable FilterDefault<R>(this IEnumerable data, params KeyValuePair<string, R>[] kvps) where R : IConvertible
        {
            return data.FilterByIndices(FilterIndex(data, kvps.Select(_ => _.Key), kvps.Select(_ => _.Value)));
        }

        public static IEnumerable<int> FilterIndex<R>(this IEnumerable data, IEnumerable<string> filter, IEnumerable<R> filterOn) where R : IConvertible
        {
            IEnumerator<string> fenm = (filter).GetEnumerator();
            IEnumerator<R> fenom = (filterOn).GetEnumerator();

            var filtered = new int[] { };

            while (fenm.MoveNext() && fenom.MoveNext())
            {
                var filted = data.GetPropertyValues<R>(fenm.Current).ToList();
                filtered = UtilityHelper.Generic.Filter.Get(filted, fenom.Current).Union(filtered).ToArray();
            }

            return filtered;
        }
    }
}