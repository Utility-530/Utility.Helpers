using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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






    }


    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T, int> action)
        {
            // argument null checking omitted
            int i = 0;
            foreach (T item in sequence)
            {
                action(item, i);
                i++;
            }
        }


    }






}
