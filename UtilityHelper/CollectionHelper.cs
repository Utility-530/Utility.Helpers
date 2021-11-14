using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Utility
{
    public static class CollectionHelper
    {
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is IList<T> list)
            {
                return new ReadOnlyCollection<T>(list);
            }
            return new ReadOnlyCollection<T>(new List<T>(enumerable));
        }


        public static ReadOnlyCollection<T> EmptyReadOnly<T>()
        {
            return new ReadOnlyCollection<T>(new T[] { });
        }
    }
}
