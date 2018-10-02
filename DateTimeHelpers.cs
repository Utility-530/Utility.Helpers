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

        public static int GetDayInterval(this DateTime date)
        {

            DateTime startDate = new DateTime(1970, 7, 1);
            return date.Subtract(startDate).Days % 365;


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


        public static IEnumerable<TimeSpan> SelectDifferences(this IEnumerable<DateTime> sequence)
        {
            using (var e = sequence.GetEnumerator())
            {
                e.MoveNext();
                DateTime last = e.Current;
                while (e.MoveNext())
                {
                    yield return e.Current - last;
                    last = e.Current;
                }

            }
        }

        //https://stackoverflow.com/questions/8847679/find-average-of-collection-of-timespans
        //answered Jan 13 '12 at 8:23  vc 74
        public static TimeSpan Average(this IEnumerable<TimeSpan> sourceList)
        {
            double doubleAverageTicks = sourceList.Average(timeSpan => timeSpan.Ticks);
            return new TimeSpan(Convert.ToInt64(doubleAverageTicks));
        }


        // Reschedules timeseries so that the average time increments corresponds to 1 second and the start time is now
        public static IEnumerable<KeyValuePair<DateTime, double>> Reschedule(this IEnumerable<KeyValuePair<DateTime, double>> _)
        {
            var avdiff = _.Select(ac => ac.Key).ToList().SelectDifferences().Average();
            var scalefactor = ((double)TimeSpan.TicksPerSecond) / ((double)avdiff.Ticks);
            var x = _.Select(s => new KeyValuePair<DateTime, double>(s.Key.Scale(scalefactor), s.Value));
            var y = x.Min(ad => ad.Key);
            var z = DateTime.Now - y;
            return x.Select(dd => new KeyValuePair<DateTime, double>(dd.Key + z, dd.Value));

        }




        public static string MonthName(this DateTime date)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month);
        }

        public static DateTime Period(this DateTime date, int periodInDays)
        {
            var startDate = new DateTime();
            var myDate = new DateTime(date.Year, date.Month, date.Day);
            var diff = myDate - startDate;
            return myDate.AddDays(-(diff.TotalDays % periodInDays));
        }

        public static DateTime? ToDMY(this DateTime? dateTimeNullable)
        {
            if (dateTimeNullable == null)
                return null;

            var date = (DateTime)dateTimeNullable;
            date = new DateTime(date.Year, date.Month, date.Day);
            return date;
        }

        public static DateTime ToDMY(this DateTime dateTime)
        {
            var date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            return date;
        }

        public static int GetTwoLetterYear(int fourLetterYear)
        {
            return Convert.ToInt32(fourLetterYear.ToString().Substring(2, 2));
        }


        public static bool IsYear(string syear, int min = 0, int max = 10000)
        {
            if (!int.TryParse(syear, out int year)) return false;
            return (year > min & year < max);
        }

        /// <summary>
        /// Get range of dates between the startdate and enddate
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> Range(this DateTime startDate, DateTime endDate)
        {
            return Enumerable.Range(0, (int)(endDate - startDate).TotalDays + 1).Select(i => startDate.AddDays(i));
        }



   
    }

}
