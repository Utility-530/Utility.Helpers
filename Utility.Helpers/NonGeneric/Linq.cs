using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility.Helpers.NonGeneric
{
    public static class Linq
    {
        public static void DynamicUsing(object resource, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                (resource as IDisposable)?.Dispose();
            }
        }

        public static int Count(this IEnumerable source)
        {
            if (source is ICollection col)
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

        public static object? ElementAt(this IEnumerable source, int index)
        {
            if (source is Array col)
                return col.GetValue(index);

            int c = 0;
            var e = source.GetEnumerator();
            object? element = null;

            DynamicUsing(e, () =>
            {
                while (e.MoveNext())
                {
                    if (c == index)
                    {
                        element = e.Current;
                        break;
                    }
                    c++;
                }
            });

            return element;
        }

        //public static int Count(this IEnumerable enumerable)
        //{
        //    IEnumerator enumerator = enumerable.GetEnumerator();
        //    int i = 0;
        //    while (enumerator.MoveNext())
        //    {
        //        i++;
        //    }
        //    return i;
        //}

        //public static object First(this IEnumerable enumerable)
        //{
        //    IEnumerator enumerator = enumerable.GetEnumerator();
        //    enumerator.MoveNext();
        //    return enumerator.Current;
        //}
        public static bool Any(this IEnumerable enumerable)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();            
            return enumerator.MoveNext();
        }


        public static object First(this IEnumerable enumerable)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static IEnumerable<bool> MoveAll(this IEnumerable<IEnumerator> enumerators)
        {
            foreach (var x in enumerators)
            {
                yield return x.MoveNext();
            }
        }

        public static IEnumerable GetCurrent(this IEnumerable<IEnumerator> enumerators)
        {
            foreach (var x in enumerators)
            {
                yield return x.Current;
            }
        }

        public static IEnumerable[] SplitInTwo(this IEnumerable collection, double ratio)
        {
            int chunkCount = (int)(collection.Count() * ratio);

            return new[] { collection.Take(chunkCount), collection.Skip(chunkCount) };
        }

        public static IEnumerable Skip(this IEnumerable collection, int chunkCount)
        {
            IEnumerator enumerator = collection.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (i > chunkCount)
                    yield return enumerator.Current;
                i++;
            }
        }

        public static IEnumerable Take(this IEnumerable collection, int chunkCount)
        {
            IEnumerator enumerator = collection.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (i < chunkCount)
                    yield return enumerator.Current;
                i++;
            }
        }
    }
}