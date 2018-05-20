using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{

    // https://stackoverflow.com/questions/141467/recursive-list-flattening

    public static class RecursiveHelper
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



        public static IEnumerable<T> RecursiveSelector<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> selector)
        {
            if (nodes.Any(_=>_!=null))
                return nodes.Concat(nodes.SelectMany(selector).RecursiveSelector(selector));

            return nodes;
        }

        //e.g var flattened = ar.RecursiveSelector(x => x.Children).ToList();

        public static List<IEnumerable<T>> RecursiveListSelector<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> selector, List<IEnumerable<T>> nodesList = null)
        {
            if (nodes.Any())
            {
                nodesList.Add(nodes.SelectMany(selector));
                nodesList.Last().RecursiveListSelector(selector, nodesList);
            }
            return nodesList;
        }
    }
}

