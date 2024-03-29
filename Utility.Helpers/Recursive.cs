﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Helpers
{
    // https://stackoverflow.com/questions/141467/recursive-list-flattening

    public static class Recursive
    {
        public static IEnumerable Flatten(this IEnumerable enumerable)
        {
            foreach (object element in enumerable)
            {
                if (element is IEnumerable candidate)
                {
                    foreach (object nested in Flatten(candidate))
                    {
                        yield return nested;
                    }
                }
                else
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<T> FilterSelect<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> selector)
        {
            if (nodes.Any(a => a != null))
                return nodes.Concat(nodes.SelectMany(selector).FilterSelect(selector));

            return nodes;
        }

        //e.g var flattened = ar.RecursiveSelector(x => x.Children).ToList();

        public static List<IEnumerable<T>>? SelectList<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> selector, List<IEnumerable<T>>? nodesList = null)
        {
            if (nodes.Any())
            {
                nodesList?.Add(nodes.SelectMany(selector));
                nodesList.Last().SelectList(selector, nodesList);
            }
            return nodesList;
        }
    }
}