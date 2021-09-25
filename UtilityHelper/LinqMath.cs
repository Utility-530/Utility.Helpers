using System;
using System.Collections.Generic;
using System.Linq;
using UtilityHelper;
using UtilityHelper.Generic;

namespace UtilityHelper
{
    public static class LinqMath
    {
        //moves the window in which weighted average values are taken
        public static List<double> MovingWeightedAverage<T>(this IEnumerable<T> series, int period, Func<T, double> value, Func<T, double> weight)
        {
            return series.Skip(period - 1).Aggregate(
        new
        {
            Result = new List<double>(),
            Working = new Queue<T>(series.Take(period - 1))
        },
        (list, item) =>
        {
            list.Working.Enqueue(item);
            list.Result.Add(list.Working.WeightedAverage(value, weight));
            list.Working.Dequeue();
            return list;
        }
        ).Result;
        }

        public static List<double> MovingAverage(this IEnumerable<double> series, int period)
        {
            return series.Skip(period - 1).Aggregate(
        new
        {
            Result = new List<double>(),
            Working = new Queue<double>(series.Take(period - 1).Select(item => item))
        },
        (list, item) =>
        {
            list.Working.Enqueue(item);
            list.Result.Add(list.Working.Average());
            list.Working.Dequeue();
            return list;
        }
        ).Result;
        }

        // equivalent to running-profit if records = trades (value = purchase-price, weight = quantity) and control = actual-price
        public static IEnumerable<double> RunningWeightedDifference<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight, IEnumerable<double> control)
        {
            var runningweightedvaluesum = 0d;
            var runningweightsum = 0d;

            using (var x = records.GetEnumerator())
            using (var y = control.GetEnumerator())
            {
                while (x.MoveNext() && y.MoveNext())
                {
                    runningweightedvaluesum += value(x.Current) * weight(x.Current);
                    runningweightsum += weight(x.Current);
                    if (runningweightedvaluesum != 0)
                        yield return (runningweightedvaluesum - y.Current) / runningweightsum;
                    else
                        yield return 0;
                }
            }
        }
    }
}