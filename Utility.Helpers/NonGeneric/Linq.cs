using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Utility.Helpers.NonGeneric
{
    public static class Linq
    {


        public static IEnumerable ForEach(this IEnumerable collection, Action<object> action)
        {
            if (collection is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    action(list[i]);
                }
            }
            else
            {
                foreach (var item in collection)
                {
                    action(item);
                }
            }

            return collection;
        }

        public static void ForEach<T>(this IEnumerable collection, Action<T> action)
        {
            if (collection is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    action((T)list[i]);
                }
            }
            else
            {
                foreach (T item in collection)
                {
                    action(item);
                }
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
                (resource as IDisposable)?.Dispose();
            }
        }



        public static object? ElementAt(this IEnumerable source, int index)
        {
            if (source is Array col)
                return col.GetValue(index);

            int i = 0;
            var e = source.GetEnumerator();
            object? element = null;

            DynamicUsing(e, () =>
            {
                while (e.MoveNext())
                {
                    if (i == index)
                    {
                        element = e.Current;
                        break;
                    }
                    i++;
                }
            });

            return element;
        }

        public static IEnumerable WhereNotAt(this IEnumerable source, int index)
        {
            int i = 0;
            var e = source.GetEnumerator();
            ArrayList arrayList = new();
            DynamicUsing(e, () =>
            {
                while (e.MoveNext())
                {
                    if (i == index)
                    {

                    }
                    else
                    {
                        arrayList.Add(e.Current);
                    }
                    i++;
                }
            });
            return arrayList;

        }

        public static void RemoveAt(this IEnumerable source, int index)
        {
            if (source is IList list)
            {
                list.RemoveAt(index);
                return;
            }
            throw new Exception("2 2£ £");
        }

        public static void Remove<T>(this IEnumerable source)
        {
            source.RemoveWhere(x => x is T);
        }



        public static IEnumerable RemoveWhere(this IEnumerable collection, Predicate<object> predicate)
        {
            if (collection is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (predicate(list[i]))
                    {
                        list.RemoveAt(i--);
                    }
                    else
                        yield return list[i];
                }
            }

            foreach (var e in collection)
            {
                if (predicate(e))
                {
                    yield return e;
                }
            }
        }

        public static int IndexOf(this IEnumerable source, object value, IEqualityComparer? comparer = null)
        {
            comparer ??= EqualityComparer<object>.Default;
            return IndexOf(source, a => comparer.Equals(value, a));
        }

        public static int IndexOf(this IEnumerable source, Func<object, object> keySelector, object key)
        {
            return IndexOf(source, a => keySelector(a).Equals(key));
        }

        public static int IndexOf(this IEnumerable source, Predicate<object> predicate)
        {
            int i = 0;
            int index = -1;
            var e = source.GetEnumerator();

            DynamicUsing(e, () =>
            {
                while (e.MoveNext())
                {
                    if (predicate(e.Current))
                    {
                        index = i;
                        break;
                    }
                    i++;
                }
            });

            return index;
        }

        public static int Count(this IEnumerable source, Predicate<object>? predicate = null)
        {
            if (source is ICollection col)
                return col.Count;
            predicate ??= new Predicate<object>(a => true);
            int count = 0;
            var enumerator = source.GetEnumerator();
            DynamicUsing(enumerator, () =>
            {
                while (enumerator.MoveNext())
                    if (predicate(enumerator.Current))
                        count++;
            });

            return count;
        }

        public static bool Any(this IEnumerable enumerable, Predicate<object>? predicate = null)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            predicate ??= new Predicate<object>(a => true);
            while (enumerator.MoveNext())
                if (predicate(enumerator.Current))
                    return true;
            return false;
        }


        public static object First(this IEnumerable enumerable, Predicate<object>? predicate = null)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            predicate ??= new Predicate<object>(a => true);
            while (enumerator.MoveNext())
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            throw new Exception(" rr3423322222");
        }

        public static object? FirstOrDefault(this IEnumerable enumerable, Predicate<object>? predicate = null)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            predicate ??= new Predicate<object>(a => true);
            while (enumerator.MoveNext())
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            return null;
        }

        public static IEnumerable Where(this IEnumerable enumerable, Predicate<object>? predicate = null)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            predicate ??= new Predicate<object>(a => true);
            while (enumerator.MoveNext())
                if (predicate(enumerator.Current))
                    yield return enumerator.Current;
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

        public static IEnumerable[] SplitInTwo(this IEnumerable collection, double ratio = 0.5)
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

        public static Collection<T> ToCollection<T>(this IEnumerable enumerable)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            var collection = new Collection<T>();
            while (enumerator.MoveNext())
            {
                collection.Add((T)enumerator.Current);
            }
            return collection;
        }
    }
}