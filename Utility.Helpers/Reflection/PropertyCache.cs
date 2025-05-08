using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utility.Helpers;

namespace Utility.Helpers.Reflection
{
    public class PropertyCache<R> where R : notnull
    {
        private readonly Dictionary<string, Func<R, object>> dictionary = new();


        public static IEnumerable RecursivePropertyValues(object e, string path)
        {
            List<IEnumerable> lst = new List<IEnumerable>();
            lst.Add(new[] { e });
            try
            {
                var xx = e.GetPropertyRefValue<IEnumerable>(path);
                foreach (var x in xx)
                    lst.Add(RecursivePropertyValues(x, path));
            }
            catch (Exception ex)
            {
                //
            }
            return lst.SelectMany(a => a.Cast<object>());
        }

        public PropertyCache()
        {
            dictionary = typeof(R).GetProperties().ToDictionary(a => a.Name, a => a.ToGetter<R, object>());
        }

        public T? GetPropertyValue<T>(R obj, string name) where T : struct => (T?)dictionary[name](obj);

        public T? GetPropertyRefValue<T>(R obj, string name) where T : class => (T?)dictionary[name](obj);

        public IEnumerable<string> GetValues<T>(R obj) => dictionary.Select(a => a.Value(obj).ToString());

        public IEnumerable<T?> GetPropertyValues<T>(IEnumerable<R> obj, string name) where T : struct => obj.Select(r => GetPropertyValue<T>(r, name));

        public IEnumerable<T?> GetPropertyRefValues<T>(IEnumerable<R> obj, string name) where T : class => obj.Select(r => GetPropertyRefValue<T>(r, name));

        public string[] PropertyNames => dictionary.Keys.Cast<string>().ToArray();
    }
}