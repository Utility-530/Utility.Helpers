using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Returns the week of the year for a given Date
        /// Iso8601
        /// assumes that weeks start with Monday.
        /// Week 1 is the 1st week of the year with a Thursday in it.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static byte GetWeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return (byte)CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }


        public static DateTime GetDateTimeFromDayOfCurrentMonth(int day)
        {
            var dtt = DateTime.Today;

            var dt = new DateTime(dtt.Year, dtt.Month, day);

            return dt;

        }

        public static DayOfWeek ToDayOfWeek(string dayofweek)
        {
            var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
            DayOfWeek day = DayOfWeek.Friday;

            if (dayofweek.ToLower() == "yesterday")
                //var tomorrow = today.AddDays(1);
                day = DateTime.Now.AddDays(-1).DayOfWeek;
            else if (dayofweek.ToLower() == "tomorrow")
                day = DateTime.Now.AddDays(1).DayOfWeek;

            else if (dayofweek.ToLower() == "today")
                day = DateTime.Now.DayOfWeek;
            else
                day = daysOfWeek.First(d => d.ToString().StartsWith(dayofweek));

            return day;


        }



        public static DateTime GetPreviousWeekDayDate(this DateTime dt, DayOfWeek dow)
        {

            DateTime previousWeekday = dt.AddDays(-1);
            while (previousWeekday.DayOfWeek != dow)
                previousWeekday = previousWeekday.AddDays(-1);
            return previousWeekday;
        }

        public static DateTime GetNextWeekDayDate(this DateTime dt, DayOfWeek dow)
        {

            DateTime nextWeekDay = dt.AddDays(1);
            while (nextWeekDay.DayOfWeek != dow)
                nextWeekDay = nextWeekDay.AddDays(1);
            return nextWeekDay;
        }





        public static bool Parse(string s, string format, out DateTime dt)
        {
            return DateTime.TryParseExact(
                s,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt
            );
        }




        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }




        public static DateTime Scale(this DateTime dt, double number)
        {
            return new DateTime((long)(dt.Ticks * number));
        }


   
    }

}
