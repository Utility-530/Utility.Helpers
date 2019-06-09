using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityModel;

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

