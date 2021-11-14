using System;
using System.Collections.Generic;
using Endless.Functional;

namespace Utility
{
    public static class RandomHelper
    {

        public static string NextShortKey(int removeAfter = 6)
        {
            return Guid.NewGuid().ToString().Remove(removeAfter);
        }

        public const double Epsilon = 0.000001;

        public static int NextSign(this Random random, int factor = 1)
        {
            var result = random.Next(0, 2) * 2 - 1;
            return factor * result;
        }

        public static double NextSignDouble(this Random random, double factor = 1d)
        {
            var result = random.Next(0, 2) * 2 - 1;
            return factor * result * random.NextDouble();
        }

        public static double NextValue(double current, Random random, double min, double max)
        {
            double increment = random.NextDouble() / 50;
            double sign = random.NextSign();
            if (sign > 0)
            {
                current += increment;
                while (current > max)
                    current -= increment;
            }
            else
            {
                current -= increment;
                while (current < min)
                    current += increment;
            }

            return current;
        }

        public static T NextEnumValue<T>(Random random, Dictionary<Type, Array>? cache = null) where T : Enum
        {
            if (cache?.ContainsKey(typeof(T)) == false)
            {
                cache[typeof(T)] = Enum.GetValues(typeof(T));
            }
            return NewMethod(random, cache).Pipe(a => (T?)a.GetValue(random.Next(a.Length))) ?? throw new Exception("Unexpected null");

            static Array NewMethod(Random random, Dictionary<Type, Array>? cache) 
            {
                if (cache == null)
                    return Enum.GetValues(typeof(T));
                else
                    return (cache[typeof(T)]);
            }
        }

        public static double NextLevyValue(Random random, double c = 5.5, double mu = 1)
        {
            double u, v, t, s;

            u = Math.PI * (random.NextDouble() - 0.5);

            // the Cauchy case
            if (Math.Abs(mu - 1) < Epsilon)
            {
                t = Math.Tan(u);
                return c * t;
            }

            do
            {
                v = -Math.Log(random.NextDouble());
            } while (Math.Abs(v - 0) < Epsilon);

            // the Gaussian case
            if (Math.Abs(mu - 2) < Epsilon)
            {
                t = 2 * Math.Sin(u) * Math.Sqrt(v);
                return c * t;
            }

            // the general case
            t = Math.Sin(mu * u) / Math.Pow(Math.Cos(u), 1 / mu);
            s = Math.Pow(Math.Cos((1 - mu) * u) / v, (1 - mu) / mu);

            return c * t * s;
        }
    }
}
