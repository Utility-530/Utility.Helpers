using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{



    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // If null then it lasts forever

    }

   
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
                    return new DateRange { StartDate = other.StartDate, EndDate = one.EndDate };
                else
                    return new DateRange { StartDate = one.StartDate, EndDate = other.EndDate };
            else if (one.HasFullOverLapWith(other))
                if (one.DoesStartBeforeStartOf(other))
                    return new DateRange { StartDate = other.StartDate, EndDate = other.EndDate };
                else
                    return new DateRange { StartDate =one.StartDate, EndDate = one.EndDate };
            else
               return null;


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
            return one.StartDate > other.GetNullSafeEndDate();
        }

        private static bool DoesStartBeforeStartOf(this DateRange one, DateRange other)
        {
            return one.StartDate <= other.StartDate;
        }


        private static bool DoesStartBeforeEndOf(this DateRange one, DateRange other)
        {
            return one.StartDate < other.GetNullSafeEndDate();
        }


        private static bool DoesEndBeforeEndOf(this DateRange one, DateRange other)
        {
            return one.GetNullSafeEndDate() <= other.GetNullSafeEndDate();
        }

        private static bool DoesEndBeforeStartOf(this DateRange one, DateRange other)
        {
            return one.GetNullSafeEndDate() < other.StartDate;
        }


        public static DateTime GetNullSafeEndDate(this DateRange one)

        { return one.EndDate ?? DateTime.MaxValue; }



    }
}
