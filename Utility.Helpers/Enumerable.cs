using System;
using System.Collections.Generic;

namespace Utility.Helpers
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Create<T>(int count, Func<T> create)
        {
            for (int i = 0; i < count; i++)
            {
                yield return create();
            }
        }


        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection is IList<T> list)
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

    }
}