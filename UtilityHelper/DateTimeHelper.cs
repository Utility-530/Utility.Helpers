using System;
using System.Globalization;
using Utility.Abstract;

namespace Utility
{
    /// <summary>
    /// Utility class related to <c>DateTime</c>.
    /// </summary>
    public static class DateTimeHelper
    {
        public static int GetQuarter(int nMonth)
        {
            if (nMonth <= 3)
                return 1;
            if (nMonth <= 6)
                return 2;
            return nMonth <= 9 ? 3 : 4;
        }

        public static long GetDateDifference(UtilityEnum.TimeInterval intervalType, DateTime startDate, DateTime endDate)
        {
            return GetDateDifference(intervalType, startDate, endDate, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }

        public static long GetDateDifference(UtilityEnum.TimeInterval intervalType, DateTime startDate, DateTime endDate, DayOfWeek firstDayOfWeek)
        {
            return intervalType switch
            {
                UtilityEnum.TimeInterval.Year => endDate.Year - startDate.Year,
                UtilityEnum.TimeInterval.Month => (endDate.Month - startDate.Month) + (12 * (endDate.Year - startDate.Year)),
                _ => GetDifference(endDate - startDate)
            };

            long GetDifference(TimeSpan ts)
            {
                switch (intervalType)
                {
                    case UtilityEnum.TimeInterval.Day:
                    //case UtilityEnum.TimeInterval.DayOfYear:
                        return Round(ts.TotalDays);

                    case UtilityEnum.TimeInterval.Hour:
                        return Round(ts.TotalHours);

                    case UtilityEnum.TimeInterval.Minute:
                        return Round(ts.TotalMinutes);

                    case UtilityEnum.TimeInterval.Second:
                        return Round(ts.TotalSeconds);

                    //case UtilityEnum.TimeInterval.Weekday:
                    //    return Round(ts.TotalDays / 7.0);

                    //case UtilityEnum.TimeInterval.WeekOfYear:
                    //{
                    //    while (endDate.DayOfWeek != firstDayOfWeek) endDate = endDate.AddDays(-1);
                    //    while (startDate.DayOfWeek != firstDayOfWeek) startDate = startDate.AddDays(-1);
                    //    ts = endDate - startDate;
                    //    return Round(ts.TotalDays / 7.0);
                    //}
                    //case UtilityEnum.TimeInterval.Quarter:
                    //{
                    //    var d1Quarter = GetQuarter(startDate.Month);
                    //    var d2Quarter = GetQuarter(endDate.Month);
                    //    var d1 = d2Quarter - d1Quarter;
                    //    var d2 = (4 * (endDate.Year - startDate.Year));
                    //    return Round(d1 + d2);
                    //}
                    case UtilityEnum.TimeInterval.Month:
                    case UtilityEnum.TimeInterval.Year:
                    default:
                        return 0;
                }
            }
        }

        public static long Round(double dVal)
        {
            if (dVal >= 0) return (long)System.Math.Floor(dVal);
            return (long)System.Math.Ceiling(dVal);
        }


    }
}

