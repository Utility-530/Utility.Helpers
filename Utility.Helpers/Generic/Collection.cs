using System;
using System.Collections.Generic;
using System.Linq;
using Utility.Helpers;
using Utility.Helpers.Generic;
using Utility.Helpers.NonGeneric;

namespace Utility.Helpers.Generic
{
    public static class CollectionExtension
    {
        public static void Then<T>(this T caller, Action<T> action) => action?.Invoke(caller);

        public static bool Then(this bool condition, Action action)
        {
            if (condition)
            {
                action();
            }

            return condition;
        }

        public static bool Else(this bool condition, Action action)
        {
            if (!condition)
            {
                action();
            }

            return condition;
        }

        public static T SingleOrAdd<T>(this ICollection<T> query, T x) where T : new()
        {
            var xd = query.SingleOrDefault(null);
            if (xd == null) query.Add(x);

            return xd;
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            values.ForEach(collection.Add);
            return collection;
        }

        public static ICollection<T> RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            values.ForEach(v => collection.Remove(v));
            return collection;
        }

        public static ICollection<T> RemoveOne<T>(this ICollection<T> collection, Func<T, bool> search)
        {
            if (collection.FirstOrDefault(search) is { } x)
            {
                collection.Remove(x);
            }
            return collection;
        }

        public static ICollection<T> RemoveBy<T>(this ICollection<T> collection, Func<T, bool> search)
        {
            List<T> list = new();
            foreach (var x in collection.Where(search))
            {
                list.Add(x);
            }

            foreach (var x in list)
                collection.Remove(x);

            return collection;
        }

        public static SortedList<DateTime, double> MovingAverage(this SortedList<DateTime, double> series, int period)
        {
            return new SortedList<DateTime, double>(series.Skip(period - 1).Scan(new SortedList<DateTime, double>(),

                 (list, item) => { list.Add(item.Key, item.Value); return list; })
                .Select(a => new KeyValuePair<DateTime, double>(a.Last().Key, a.Select(a => a.Value).Average()))
                .ToDictionary(a => a.Key, a => a.Value));
        }
    }
}