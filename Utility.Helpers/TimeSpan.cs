using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Helpers
{
    public static class TimeSpanHelper
    {
        //https://stackoverflow.com/questions/8847679/find-average-of-collection-of-timespans
        //answered Jan 13 '12 at 8:23  vc 74
        public static TimeSpan Average(this IEnumerable<TimeSpan> sourceList) => new TimeSpan(Convert.ToInt64(sourceList.Average(timeSpan => timeSpan.Ticks)));
    }
}