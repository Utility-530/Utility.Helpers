﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class RandomHelper
    {

        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> x, double percent, Random rand = null)
        {
            rand = rand ?? new Random();

            using (var e = x.GetEnumerator())
            {
                while (e.MoveNext())
                    if (rand.Next(100) < percent)
                        yield return e.Current;

            }


        }
        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> x, int count, Random rand = null)
        {

            return x.OrderBy(_ => rand.Next()).Take(count);

        }

        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> x, int count)
        {

            return x.OrderBy(arg => Guid.NewGuid()).Take(count);
        }



        public static IEnumerable<int> GetRandomNumbersInRange(int size, int percent, Random rand = null)
        {
            rand = rand ?? new Random();

            for (int i = 0; i < size; i++)
            {
                if (rand.Next(100) > percent)
                {
                    yield return i;
                }
            }
        }





    }
}
