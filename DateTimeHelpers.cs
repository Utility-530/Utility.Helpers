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




        public static bool CheckDate1(string date, out DateTime result)
        {
            string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                   "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};

            result = default(DateTime);
            foreach (string s in regexstrings)
            {

                DateTime.TryParseExact(date, formats,
                                                CultureInfo.InvariantCulture,
                                                  DateTimeStyles.None,
                                                  out result);
                return true;

            }
            return false;
            //Match m = Regex.Match(example, @"^(?<day>\d\d?)-(?<month>\d\d?)-(?<year>\d\d\d\d)$");
            //string strDay = m.Groups["day"].Value;
            //string strMonth = m.Groups["month"].Value;
            //string strYear = m.Groups["year"].Value;
        }


        public static bool CheckDate2(string date, out DateTime result)
        {
            result = default(DateTime);
            foreach (string s in regexstrings)
            {
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(date, s);
                if (DateTime.TryParse(m.Value, out result))
                    return true;

            }
            return false;
            //Match m = Regex.Match(example, @"^(?<day>\d\d?)-(?<month>\d\d?)-(?<year>\d\d\d\d)$");
            //string strDay = m.Groups["day"].Value;
            //string strMonth = m.Groups["month"].Value;
            //string strYear = m.Groups["year"].Value;
        }

        //http://regexlib.com/DisplayPatterns.aspx?cattabindex=4&categoryId=5&AspxAutoDetectCookieSupport=1

        //This RE validates dates in the dd MMM yyyy format. Spaces separate the values.
        const string regex1 = @"^((31(?!\ (Feb(ruary)?|Apr(il)?|June?|(Sep(?=\b|t)t?|Nov)(ember)?)))|((30|29)(?!\ Feb(ruary)?))|(29(?=\ Feb(ruary)?\ (((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)))))|(0?[1-9])|1\d|2[0-8])\ (Jan(uary)?|Feb(ruary)?|Ma(r(ch)?|y)|Apr(il)?|Ju((ly?)|(ne?))|Aug(ust)?|Oct(ober)?|(Sep(?=\b|t)t?|Nov|Dec)(ember)?)\ ((1[6-9]|[2-9]\d)\d{2})$";
        //Matches ANSI SQL date format YYYY-mm-dd hh:mi:ss am/pm. You can use / - or space for date delimiters, so 2004-12-31 works just as well as 2004/12/31. Checks leap year from 1901 to 2099.
        const string regex2 = @"^((\d{2}(([02468][048])|([13579][26]))[\-\/\s]?((((0?[13578])|(1[02]))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\-\/\s]?((0?[1-9])|([1-2][0-9])))))|(\d{2}(([02468][1235679])|([13579][01345789]))[\-\/\s]?((((0?[13578])|(1[02]))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\-\/\s]?((0?[1-9])|(1[0-9])|(2[0-8]))))))(\s(((0?[1-9])|(1[0-2]))\:([0-5][0-9])((\s)|(\:([0-5][0-9])\s))([AM|PM|am|pm]{2,2})))?$";
        //yyMMdd with leap years. Minimized expression. As we have only 2 numbers for the years, dates 1600, 2000, etc are still validated.
        const string regex3 = @"^(\d{2}((0[1-9]|1[012])(0[1-9]|1\d|2[0-8])|(0[13456789]|1[012])(29|30)|(0[13578]|1[02])31)|([02468][048]|[13579][26])0229)$";



        public static string[] regexstrings => new[] { regex1, regex2, regex3 };

    }

}
