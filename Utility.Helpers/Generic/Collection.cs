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

        public static SortedList<DateTime, double> MovingAverage(this SortedList<DateTime, double> series, int period)
        {
            return new SortedList<DateTime, double>(series.Skip(period - 1).Scan(new SortedList<DateTime, double>(),

                 (list, item) => { list.Add(item.Key, item.Value); return list; })
                .Select(a => new KeyValuePair<DateTime, double>(a.Last().Key, a.Select(a => a.Value).Average()))
                .ToDictionary(a => a.Key, a => a.Value));
        }
    }
}