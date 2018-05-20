using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }




        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }



        public static TSource Pernultimate<TSource>(this IEnumerable<TSource> source)
        {
            //from http://stackoverflow.com/questions/8724179/linq-how-to-get-second-last
            return source.Reverse().Skip(1).Take(1).FirstOrDefault();
        }



        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int n)
        {
            int count = source.Count();

            if (source == null)
                throw new ArgumentNullException("collection");
            if (n < 0)
                throw new ArgumentOutOfRangeException("n", "n must be 0 or greater");

       
            int i = 0;
            foreach (T result in source)
            {
                if (++i == count-n) //this is the last item
                    yield return result;
            }
        }



    }

}
