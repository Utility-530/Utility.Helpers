using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility.Helpers.NonGeneric
{
    public static class ListHelper
    {
        //public static object SingleOrAdd(this ICollection query, object x)
        //{
        //    var xd = query.SingleOrDefault(null);
        //    if (xd == null) query.Add(x);

        //    return xd;
        //}

        public static IList AddRange(this IList collection, IEnumerable values)
        {
            values.ForEach((a) => collection.Add(a));
            return collection;
        }

        public static ICollection RemoveRange(this IList collection, IEnumerable values)
        {
            values.ForEach(collection.Remove);
            return collection;
        }

        public static ICollection RemoveOne(this IList collection, Predicate<object> search)
        {
            if (collection.FirstOrDefault(search) is { } x)
            {
                collection.Remove(x);
            }
            return collection;
        }

        public static ICollection RemoveBy(this IList collection, Predicate<object> search)
        {
            IList list = new List<object>();
            foreach (var x in collection.Where(search))
            {
                list.Add(x);
            }

            foreach (var x in list)
                collection.Remove(x);

            return collection;
        }
    }
}