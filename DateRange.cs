using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UtilityStruct;

namespace UtilityHelper
{
    public static class DateRangeHelper
    {
        public static bool HasOverLapWith(this IEnumerable<DateRange> membershipList, DateRange newItem)
        {
            return membershipList.Any(m => m.HasOverLapWith(newItem));
            //return !membershipList.All(m => m.IsFullyAfter(newItem) || newItem.IsFullyAfter(m));
            //return membershipList.Any(m => m.HasPartialOverLapWith(newItem) || newItem.HasFullOverLapWith(newItem));
        }

        public static DateRange GetoverLapWith(this DateRange one, DateRange other)
        {
            if (one.HasPartialOverLapWith(other))
                if (one.DoesStartBeforeStartOf(other))
                    return new DateRange(other.Start, one.End);
                else
                    return new DateRange(one.Start, other.End);
            else if (one.HasFullOverLapWith(other))
                if (one.DoesStartBeforeStartOf(other))
                    return new DateRange(other.Start, other.End);
                else
                    return new DateRange(one.Start, one.End);
            else
                return default(DateRange);
        }

        public static IEnumerable<IGrouping<DateRange, T>> GroupBy<T>(this IOrderedEnumerable<T> enumerable, TimeSpan timeSpan, Func<T, DateTime> predicate)
        {
            Grouping<T> grouping = null;
            foreach (var (a, dt) in from b in enumerable select (b, predicate.Invoke(b)))
            {
                if (grouping == null || dt > grouping.Key.End)
                    yield return grouping = new Grouping<T>(new DateRange(dt, dt + timeSpan), a);
                else
                    grouping.Add(a);
            }
        }

        private class Grouping<T> : IGrouping<DateRange, T>
        {
            private readonly List<T> elements = new List<T>();

            public DateRange Key { get; }

            public Grouping(DateRange key) => Key = key;

            public Grouping(DateRange key, T element) : this(key) => Add(element);

            public void Add(T element) => elements.Add(element);

            public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static bool HasOverLapWith(this DateRange one, DateRange other)
        {
            return !one.IsFullyAfter(other) && !other.IsFullyAfter(other);
        }

        public static bool HasPartialOverLapWith(this DateRange one, DateRange other)
        {
            return

            (one.DoesStartBeforeStartOf(other) && one.DoesEndBeforeEndOf(other) && other.DoesStartBeforeEndOf(one))
            ||
            (other.DoesStartBeforeStartOf(one) && other.DoesEndBeforeEndOf(one) && one.DoesStartBeforeEndOf(other));
        }

        public static bool HasFullOverLapWith(this DateRange one, DateRange other)
        {
            return (one.IsFullyWithin(other) || other.IsFullyWithin(one));
        }

        public static bool IsFullyWithin(this DateRange one, DateRange other)
        {
            return (other.DoesStartBeforeStartOf(one) && one.DoesEndBeforeStartOf(other));
        }

        private static bool IsFullyAfter(this DateRange one, DateRange other)
        {
            return one.Start > other.GetNullSafeEnd();
        }

        private static bool DoesStartBeforeStartOf(this DateRange one, DateRange other)
        {
            return one.Start <= other.Start;
        }

        private static bool DoesStartBeforeEndOf(this DateRange one, DateRange other)
        {
            return one.Start < other.GetNullSafeEnd();
        }

        private static bool DoesEndBeforeEndOf(this DateRange one, DateRange other)
        {
            return one.GetNullSafeEnd() <= other.GetNullSafeEnd();
        }

        private static bool DoesEndBeforeStartOf(this DateRange one, DateRange other)
        {
            return one.GetNullSafeEnd() < other.Start;
        }

        public static DateTime GetNullSafeEnd(this DateRange one)

        { return one.End == default(DateTime) ? DateTime.MaxValue : one.End; }
    }
}