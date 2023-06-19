using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace Utility.Helpers
{
    public static class DictionaryHelper
    {
        /// <summary>
        /// Get a the value for a key. If the key does not exist, creates a new one
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dic">The dictionary to call this method on.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The key value. null if this key is not in the dictionary.</returns>
        public static TValue GetValueOrNew<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key) where TValue : new()
        {
            if (dic.TryGetValue(key, out TValue result))
                return result;
            return dic[key] = new TValue(); ;
        }

        /// <summary>
        /// Get a the value for a key. If the key does not exist, creates a new one
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dic">The dictionary to call this method on.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The key value. null if this key is not in the dictionary.</returns>
        public static TValue GetValueOr<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.TryGetValue(key, out TValue result))
                return result;
            return dic[key] = value;
        }


        /// <summary>
        /// Get a the value for a key. If the key does not exist, creates a new one
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dic">The dictionary to call this method on.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The key value. null if this key is not in the dictionary.</returns>
        public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue> createValue)
        {
            if (dic.TryGetValue(key, out TValue result))
                return result;
            return dic[key] = createValue();
        }

        public static void AddRange<K, V>(this IDictionary<K, V> me, params IDictionary<K, V>[] others)
        {
            foreach (IDictionary<K, V> src in others)
            {
                foreach (KeyValuePair<K, V> p in src)
                {
                    //if(typeof(IDictionary).IsAssignableFrom(typeof(V)e()))
                    me[p.Key] = p.Value;
                }
            }
        }

        /// <summary>
        /// Unionise two dictionaries of generic types.
        /// Duplicates take their value from the leftmost dictionary.
        /// </summary>
        /// <typeparam name="T1">Generic key type</typeparam>
        /// <typeparam name="T2">Generic value type</typeparam>
        /// <param name="D1">System.Collections.Generic.Dictionary 1</param>
        /// <param name="D2">System.Collections.Generic.Dictionary 2</param>
        /// <returns>The combined dictionaries.</returns>
        public static Dictionary<T1, T2> UnionDictionaries<T1, T2>(IDictionary<T1, T2> D1, IDictionary<T1, T2> D2) where T2 : notnull
        {
            Dictionary<T1, T2> rd = new Dictionary<T1, T2>(D1);
            foreach (var key in D2.Keys)
            {
                if (!rd.ContainsKey(key))
                    rd.Add(key, D2[key]);
                else if (rd[key].GetType().IsGenericType)
                {
                    if (rd[key].GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        MethodInfo info = MethodBase.GetCurrentMethod() is MethodInfo info1 ? info1 : typeof(DictionaryHelper).GetMethod("UnionDictionaries", BindingFlags.Public | BindingFlags.Static);
                        var genericMethod = info.MakeGenericMethod(rd[key].GetType().GetGenericArguments()[0], rd[key].GetType().GetGenericArguments()[1]);
                        var invocationResult = genericMethod.Invoke(null, new object[] { rd[key], D2[key] });
                        rd[key] = (T2)invocationResult;
                    }
                }
            }
            return rd;
        }

        public static Dictionary<T, R> ToDictionary<T, R>(this IEnumerable<KeyValuePair<T, R>> kvps)
        {
            return kvps.ToDictionary(a => a.Key, a => a.Value);
        }

        public static Dictionary<T, R> ToDictionary<T, R>(this IEnumerable<Tuple<T, R>> kvps)
        {
            return kvps.ToDictionary(a => a.Item1, a => a.Item2);
        }

        public static IEnumerable<dynamic> ToDynamics<T>(this IList<Dictionary<string, T>> dics)
        {
            return dics.Select(a =>
            {
                return a.ToDynamic();
            });
        }

        public static dynamic ToDynamic<T>(this IDictionary<string, T> dict)
        {
            IDictionary<string, object?> eo = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, T> kvp in dict)
            {
                eo.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
            }
            return eo;
        }

        public static OrderedDictionary ToOrderedDictionary<TSource>(IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, object> elementSelector)
        {
            var d = new OrderedDictionary();

            foreach (TSource element in source)
            {
                var val = elementSelector(element);
                if (val != null)
                    d.Add(keySelector(element), val);
            }

            return d;
        }

        public static IEnumerable<(TKey key, TValue one, TValue two)> Differences<TKey, TValue>(
       this IDictionary<TKey, TValue> dictionary1,
       IDictionary<TKey, TValue> dictionary2) => Compare(dictionary1, dictionary2, false);
        public static IEnumerable<(TKey key, TValue one, TValue two)> Similarities<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary1,
        IDictionary<TKey, TValue> dictionary2) => Compare(dictionary1, dictionary2, true);

        public static IEnumerable<(TKey key, TValue one, TValue two)> Compare<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary1,
            IDictionary<TKey, TValue> dictionary2,
            bool match)
        {
            foreach (var key in dictionary1.Keys)
            {
                var one = dictionary1.GetValueOrDefault(key);
                var two = dictionary2.GetValueOrDefault(key);
                if (one is not null && two is not null && one.Equals(two).Equals(match) == true)
                {
                    yield return (key, one, two);
                }
            }
        }

        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
        {
            if (dictionary == null) { throw new ArgumentNullException(nameof(dictionary)); }
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}