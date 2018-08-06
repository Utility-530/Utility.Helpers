using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityHelper
{
    public struct Day
    {
        public Day(int d) { val = d; }
        public int val;

        public static implicit operator int(Day d)
        {
            return d.val;
        }

        public static implicit operator Day(int d)
        {
            return new Day(d);
        }
 
        public static implicit operator Day(DateTime d)
        {
            return new Day((int)(d - default(DateTime)).TotalDays);
        }

        //  User-defined conversion from DateTime to Day 
        public static implicit operator DateTime(Day d)
        {
            return new DateTime((d * TimeSpan.TicksPerDay));
        }
    }


}
