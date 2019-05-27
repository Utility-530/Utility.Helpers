using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilityHelper.Generic
{
    public static class Filter
    {
        public static IEnumerable<T> FilterDefault<T, R>(IEnumerable<T> data, params KeyValuePair<string, R>[] kvps)
        {
            return data.FilterByIndex(FilterIndex(data, kvps.Select(_ => _.Key), kvps.Select(_ => _.Value)));

        }


        public static IEnumerable<T> FilterWithNull<T, R>(this IEnumerable<T> data, params KeyValuePair<string, R>[] kvps) where R : IConvertible
        {
            return UtilityHelper.NonGeneric.Filter.FilterByIndex(data, FilterIndex(data, kvps.Select(_ => _.Key), kvps.Select(_ => _.Value)), true).Cast<T>();

        }

        public static IEnumerable<T> FilterByIndex<T>(this IEnumerable<T> enumerable, IEnumerable<int> indices)
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

        public static IEnumerable<T> FilterByIndex<T>(this IEnumerable<T> feed, Func<int, bool> filter)
        {
            return feed
         .Select((p, i) => new
         {
             Item = p,
             Index = i
         })
         .Where(p => filter(p.Index))
         .Select(p => p.Item);
        }


        public static IEnumerable<T> FilterWithout<T>(this IEnumerable<T> feed, int[] filter)
        {
            return feed.FilterByIndex(_ => !filter.Contains(_));
        }

        public static IEnumerable<int> FilterIndex<T, R>(this IEnumerable<T> data, IEnumerable<string> filterheaders, IEnumerable<R> filterOn)
        {

            IEnumerator<string> fenm = ((IEnumerable<string>)filterheaders).GetEnumerator();
            IEnumerator<IConvertible> fenom = ((IEnumerable<IConvertible>)filterOn).GetEnumerator();

            var filtered = new int[] { };

            while (fenm.MoveNext() && fenom.MoveNext())
            {
                var filter = data.GetPropValues<IConvertible>((string)fenm.Current);
                filtered =filter.GetFiltered(fenom.Current).Union(filtered).ToArray();
            }
            return filtered;


        }


        public static IEnumerable<int> GetFiltered<R>(this IEnumerable<R> filted, R current) where R : IConvertible
        {
            return filted
                    .Select((_, i) => _.Equals(current) ? (int?)i : null)
                    .Where(_ => _ != null)
                    .Select(a => (int)a);
        }



    }
}
