

using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    //https://gist.github.com/crmorgan/5de0359b31555a80d9d8
    //crmorgan/DateRange.cs
    /// <summary>
    ///     Represents a range of dates.
    /// </summary>
    public struct DateRange
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DateRange" /> structure to the specified start and end date.
        /// </summary>
        /// <param name="startDate">A string that contains that first date in the date range.</param>
        /// <param name="endDate">A string that contains the last date in the date range.</param>
        /// <exception cref="System.ArgumentNullException">
        ///		endDate or startDate are <c>null</c>.
        /// </exception>
        /// <exception cref="System.FormatException">
        ///     endDate or startDate do not contain a vaild string representation of a date and time.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		endDate is not greater than or equal to startDate
        /// </exception>
        public DateRange(string startDate, string endDate) : this()
        {
            if (string.IsNullOrWhiteSpace(startDate))
            {
                throw new ArgumentNullException("startDate");
            }

            if (string.IsNullOrWhiteSpace(endDate))
            {
                throw new ArgumentNullException("endDate");
            }

            Start = DateTime.Parse(startDate);
            End = DateTime.Parse(endDate);

            if (End < Start)
            {
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            }
        }

        public DateRange(DateTime startDate, DateTime endDate) : this()
        {
       

            Start = startDate;
            End = endDate;

            if (End < Start)
            {
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            }
        }
        /// <summary>
        ///     Gets the start date component of the date range.
        /// </summary>
        public DateTime Start { get; private set; }


        /// <summary>
        ///     Gets the end date component of the date range.
        /// </summary>
        public DateTime End { get; private set; }

        /// <summary>
        ///     Gets a collection of the dates in the date range.
        /// </summary>
        public IList<DateTime> Dates
        {
            get
            {
                var startDate = Start;

                return Enumerable.Range(0, Days)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList();
            }
        }

        /// <summary>
        ///     Gets the number of whole days in the date range.
        /// </summary>
        public int Days
        {
            get { return (End - Start).Days + 1; }
        }
    }
}


