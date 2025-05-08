using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utility.Helpers.Reflection;

namespace Utility.Helpers
{
    public static class ObjectToDictionaryMapper
    {
        public static Dictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static Dictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
                AddPropertyToDictionary(property, source!, dictionary);
            return dictionary;


            static void ThrowExceptionWhenSourceArgumentIsNull()
            {
                throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
            }
        }

        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            object value = property.GetValue(source);
            if (IsOfType<T>(value))
                dictionary.Add(property.Name, (T)value);
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        public static object ToObject(this Dictionary<string, object> dict, Type type)
        {
            var obj = Activator.CreateInstance(type);
            foreach (var kv in dict)
            {
                if (kv.Value != null)
                    PropertyHelper.SetValue(obj, kv.Key, kv.Value);
            }
            return obj;
        }

        public static T ToObject<T>(this Dictionary<string, object> dict)
        {
            return (T)ToObject(dict, typeof(T));
        }

    }
}