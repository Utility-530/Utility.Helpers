using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class IEnumerableExtension
    {



        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }


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



        public static void DynamicUsing(object resource, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                IDisposable d = resource as IDisposable;
                if (d != null)
                    d.Dispose();
            }
        }


        public static int Count(this IEnumerable source)
        {
            var col = source as ICollection;
            if (col != null)
                return col.Count;

            int c = 0;
            var e = source.GetEnumerator();
            DynamicUsing(e, () =>
            {
                while (e.MoveNext())
                    c++;
            });

            return c;
        }


        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> s, Func<T, U> f)
        {
            foreach (var item in s)
                yield return f(item);
        }





    }

}
