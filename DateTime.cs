using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
  //<see href = "http://www.geekzilla.co.uk/View00FF7904-B510-468C-A2C8-F859AA20581F.htm" >
  // for converting from datetime to string.



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
        public static byte GetWeekOfYear(this System.DateTime time)
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


        public static System.DateTime GetDateTimeFromDayOfCurrentMonth(int day)
        {
            var dtt = System.DateTime.Today;
            return new System.DateTime(dtt.Year, dtt.Month, day);

        }

        public static int GetDayInterval(this DateTime date) => date.Subtract(new System.DateTime(1970, 7, 1)).Days % 365;
    

        public static DayOfWeek ToDayOfWeek(string dayofweek)
        {
            DayOfWeek day = default;

            if (dayofweek.ToLower() == "yesterday")
                //var tomorrow = today.AddDays(1);
                day = System.DateTime.Now.AddDays(-1).DayOfWeek;
            else if (dayofweek.ToLower() == "tomorrow")
                day = System.DateTime.Now.AddDays(1).DayOfWeek;
            else if (dayofweek.ToLower() == "today")
                day = System.DateTime.Now.DayOfWeek;
            else
                day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayofweek);

            return day;
        }



        public static System.DateTime GetPreviousWeekDayDate(this System.DateTime dt, DayOfWeek dow)
        {
            System.DateTime previousWeekday = dt.AddDays(-1);
            while (previousWeekday.DayOfWeek != dow)
                previousWeekday = previousWeekday.AddDays(-1);
            return previousWeekday;
        }

        public static System.DateTime GetNextWeekDayDate(this System.DateTime dt, DayOfWeek dow)
        {
            System.DateTime nextWeekDay = dt.AddDays(1);
            while (nextWeekDay.DayOfWeek != dow)
                nextWeekDay = nextWeekDay.AddDays(1);
            return nextWeekDay;
        }





        public static bool TryParse(string s, string format, out System.DateTime dt) =>
            System.DateTime.TryParseExact(
                s,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt);
        



        public static System.DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }




        public static System.DateTime Scale(this System.DateTime dt, double number) => new DateTime((long)(dt.Ticks * number));



        public static IEnumerable<TimeSpan> SelectDifferences(this IEnumerable<DateTime> sequence)
        {
            using (var e = sequence.GetEnumerator())
            {
                e.MoveNext();
                System.DateTime last = e.Current;
                while (e.MoveNext())
                {
                    yield return e.Current - last;
                    last = e.Current;
                }
            }
        }


        // Reschedules timeseries so that the average time increments corresponds to 1 second and the start time is now
        public static IEnumerable<KeyValuePair<System.DateTime, double>> Reschedule(this IEnumerable<KeyValuePair<System.DateTime, double>> _)
        {
            var avdiff = _.Select(ac => ac.Key).ToList().SelectDifferences().Average();
            var scalefactor = ((double)TimeSpan.TicksPerSecond) / ((double)avdiff.Ticks);
            var x = _.Select(s => new KeyValuePair<System.DateTime, double>(s.Key.Scale(scalefactor), s.Value));
            var y = x.Min(ad => ad.Key);
            var z = System.DateTime.Now - y;
            return x.Select(dd => new KeyValuePair<System.DateTime, double>(dd.Key + z, dd.Value));

        }


        public static string MonthName(this System.DateTime date)=> CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month);
   

        public static System.DateTime Period(this System.DateTime date, int periodInDays) => date.Date.AddDays(-((date.Date - new System.DateTime()).TotalDays % periodInDays));
        


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
        public static IEnumerable<System.DateTime> Range(this System.DateTime startDate, System.DateTime endDate) => Enumerable.Range(0, (int)(endDate - startDate).TotalDays + 1).Select(i => startDate.AddDays(i));


        public static bool TryParseDate(string date, out System.DateTime result)
        {
            if (!TryParseDate1(date, out result))
            {
                return TryParseDate2(date, out result);
            }
            return false;
        }


        public static bool TryParseDate1(string date, out System.DateTime result)
        {
            string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                   "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};

            result = default;

            foreach (string s in regexstrings)
            {
               if(System.DateTime.TryParseExact(date, formats,
                                                CultureInfo.InvariantCulture,
                                                 DateTimeStyles.None,
                                                  out result))
                return true;
            }
            return false;
        }


        public static bool TryParseDate2(string date, out System.DateTime result)
        {
            result = default;
            foreach (string s in regexstrings)
            {
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(date, s);
                if (System.DateTime.TryParse(m.Value, out result))
                    return true;
            }
            return false;
        }

        //http://regexlib.com/DisplayPatterns.aspx?cattabindex=4&categoryId=5&AspxAutoDetectCookieSupport=1

        //This RE validates dates in the dd MMM yyyy format. Spaces separate the values.
        const string regex1 = @"^((31(?!\ (Feb(ruary)?|Apr(il)?|June?|(Sep(?=\b|t)t?|Nov)(ember)?)))|((30|29)(?!\ Feb(ruary)?))|(29(?=\ Feb(ruary)?\ (((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)))))|(0?[1-9])|1\d|2[0-8])\ (Jan(uary)?|Feb(ruary)?|Ma(r(ch)?|y)|Apr(il)?|Ju((ly?)|(ne?))|Aug(ust)?|Oct(ober)?|(Sep(?=\b|t)t?|Nov|Dec)(ember)?)\ ((1[6-9]|[2-9]\d)\d{2})$";
        //Matches ANSI SQL date format YYYY-mm-dd hh:mi:ss am/pm. You can use / - or space for date delimiters, so 2004-12-31 works just as well as 2004/12/31. Checks leap year from 1901 to 2099.
        const string regex2 = @"^((\d{2}(([02468][048])|([13579][26]))[\-\/\s]?((((0?[13578])|(1[02]))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\-\/\s]?((0?[1-9])|([1-2][0-9])))))|(\d{2}(([02468][1235679])|([13579][01345789]))[\-\/\s]?((((0?[13578])|(1[02]))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\-\/\s]?((0?[1-9])|(1[0-9])|(2[0-8]))))))(\s(((0?[1-9])|(1[0-2]))\:([0-5][0-9])((\s)|(\:([0-5][0-9])\s))([AM|PM|am|pm]{2,2})))?$";
        //yyMMdd with leap years. Minimized expression. As we have only 2 numbers for the years, dates 1600, 2000, etc are still validated.
        const string regex3 = @"^(\d{2}((0[1-9]|1[012])(0[1-9]|1\d|2[0-8])|(0[13456789]|1[012])(29|30)|(0[13578]|1[02])31)|([02468][048]|[13579][26])0229)$";



        static string[] regexstrings => new[] { regex1, regex2, regex3 };

    }

}
