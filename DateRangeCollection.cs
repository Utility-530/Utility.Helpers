using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace UtilityHelper
{
    // a Interval Graph would be better for this problem
    // https://en.wikipedia.org/wiki/Interval_graph
    public class DateRangeCollection : Collection<DateRange>
    {

        bool _merge;

        public DateRangeCollection(bool merge)
        {
            _merge = merge;

        }

        protected override void InsertItem(int index, DateRange member)
        {
            if (!IsOverlap(member))
                base.InsertItem(index, member);
            else
            {
                if (_merge)
                {
                    if (this.Any(_ => _.HasPartialOverLapWith(member)))
                    {
                        var x = this.First(_ => _.HasPartialOverLapWith(member));

                        if (x.StartDate > member.StartDate)
                            x.StartDate = member.StartDate;
                        else
                            x.EndDate = member.EndDate;

                        this.InsertItem(index, member);
                    }
                    else
                    {
                        var x = this.First(_ => _.HasFullOverLapWith(member));

                        if (x.IsFullyWithin(member))
                        {
                            this.Remove(x);
                            this.InsertItem(index, member);
                        }


                    }
                }
                else
                    throw new ArgumentException("Ranges cannot overlap.");

            }

        }

        public bool IsOverlap(DateRange member)
        {
            return this.HasOverLapWith(member);
        }



        //public DateRange[] GetOverlap(DateRange member)
        //{
        //    DateRange[] ts = null;

        //    if (this.HasOverLapWith(member))
        //    {
        //        foreach (var x in this)
        //        {
        //            var y = x.GetOverLap(this);



        //        }

        //        if (this.Any(_ => _.HasPartialOverLapWith(member)))
        //        {
        //            var x = this.First(_ => _.HasPartialOverLapWith(member));

        //            ts = new DateRange[1];

        //            if (x.StartDate > member.StartDate)
        //                ts[0] = (x.StartDate - member.StartDate);
        //            else
        //                ts[0] = (x.StartDate - member.StartDate);

        //        }
        //        else
        //        {
        //            var x = this.Select(_ => _.HasFullOverLapWith(member));

        //            if (x.IsFullyWithin(member))
        //            {

        //                ts = new DateRange[2];

        //                ts[0] = new DateRange { StartDate = (x.StartDate - member.StartDate), EndDate =;

        //                ts[1] = (x.EndDate - member.EndDate);

        //            }

        //        }
        //    }

        //    return ts;

        //}



    }

}
