using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityHelper;

namespace UtilityHelper
{


    public static class ListExtension
    {
        public static T GetValueOrNew<T>(this IList<T> lst, int index) where T : new()
        {
            T result;

            while (true)
            {
                try
                {
                    result = lst[index];
                    break;
                }
                catch
                {
                    lst.Add(new T());

                }
            }

            return result;
        }


        public static T SingleOrAdd<T>(this IList<T> query, T x) where T : new()
        {
            var xd = query.SingleOrDefault(null);
            if (xd == null) query.Add(x);

            return xd;

        }




        public static T SingleOrAdd<T>(this ICollection<T> query, T x) where T : new()
        {
            var xd = query.SingleOrDefault(null);
            if (xd == null) query.Add(x);

            return xd;

        }



        public static SortedList<DateTime, double> MovingAverage(this SortedList<DateTime, double> series, int period)
        {
            return new SortedList<DateTime, double>(series.Skip(period - 1).Scan(new SortedList<DateTime, double>(),

                 (list, item) => { list.Add(item.Key, item.Value); return list; })
                .Select(_ => new KeyValuePair<DateTime, double>(_.Last().Key, _.Select(__ => __.Value).Average()))
                .ToDictionary(_ => _.Key, _ => _.Value));
        }




    }







}
