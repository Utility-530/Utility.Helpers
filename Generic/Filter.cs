using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper.Generic
{
    public static class Filter
    {
        public static IEnumerable<T> FilterDefault<T, R>(IEnumerable<T> data, params KeyValuePair<string, R>[] kvps) =>
            data.FilterByIndices(Index(data, kvps.Select(_ => _.Key), kvps.Select(_ => _.Value)));

        public static IEnumerable<T> FilterWithNull<T, R>(this IEnumerable<T> data, params KeyValuePair<string, R>[] kvps) where R : IConvertible
            => UtilityHelper.NonGeneric.Filter.FilterByIndices(data, Index(data, kvps.Select(_ => _.Key), kvps.Select(_ => _.Value)), true).Cast<T>();

        public static IEnumerable<T> FilterByIndices<T>(this IEnumerable<T> enumerable, IEnumerable<int> indices)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (indices.Contains(i))
                    yield return enumerator.Current;
                i++;
            }
        }

        public static IEnumerable<T> FilterByIndices<T>(this IEnumerable<T> feed, Func<int, bool> filter)

            => from p in feed.Select((item, index) => new { item, index })
               where filter(p.index)
               select p.item;

        public static IEnumerable<T> Without<T>(this IEnumerable<T> feed, int[] filter) => feed.FilterByIndices(_ => !filter.Contains(_));

        public static IEnumerable<int> Index<T, R>(this IEnumerable<T> data, IEnumerable<string> filterheaders, IEnumerable<R> filterOn)
        {
            using (IEnumerator<string> fenm = filterheaders.GetEnumerator())
            using (IEnumerator<IConvertible> fenom = ((IEnumerable<IConvertible>)filterOn).GetEnumerator())
            {
                var filtered = new int[] { };

                while (fenm.MoveNext() && fenom.MoveNext() && fenom.Current is string current)
                {
                    var filter = data.GetPropertyRefValues<IConvertible>(current);

                    filtered = filter.OfType<IConvertible>().Select(current).Union(filtered).ToArray();
                }
                return filtered;
            }
        }

        public static IEnumerable<int> Select<R>(this IEnumerable<R> filted, R current) where R : IConvertible
        {
            return filted
                    .Select((a, i) => a.Equals(current) ? (int?)i : null)
                    .Where(a => a != null)
                    .Select(a => (int)a!);
        }
    }
}