#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Humanizer;

namespace Utility
{
    /// <summary>
    /// For converting objects into a OrderedDictionaries
    /// </summary>
    public class DictionaryConverter
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;

        public static OrderedDictionary Convert(object value, IValueConverter<(PropertyInfo, object), string?> valueConverter, IComparer<string>? comparer = null)
        {
            var arr = SelectMembers(value.GetType(), comparer).ToArray();
            var dict = DictionaryHelper.ToOrderedDictionary(
                arr,
                a => a.Name.Humanize(LetterCasing.Title),
                a => Get(valueConverter, value, a)?.ToString() ?? throw new NullReferenceException("fdsf  fry "));

            return dict;
        }

        public static OrderedDictionary ConvertMany(IEnumerable value, IValueConverter<(PropertyInfo, object), string?> valueConverter, IComparer<string>? comparer = null)
        {
            var dict = DictionaryHelper.ToOrderedDictionary(
                value.Cast<object>().SelectMany(obj =>
                {
                    var arr = SelectMembers(obj.GetType(), comparer).ToArray();
                    return arr.Select(propertyInfo => (propertyInfo, obj));
                }),
                a => a.propertyInfo.Name.Humanize(LetterCasing.Title),
                a => Get(valueConverter, a.obj, a.propertyInfo)?.ToString() ?? throw new NullReferenceException("2 fddsf  fry "));

            return dict;
        }

        public static IEnumerable<KeyValuePair<string, OrderedDictionary>> Convert(IEnumerable value, IValueConverter<(PropertyInfo, object), string?> valueConverter)
        {
            foreach (var obj in value.Cast<object>().ToArray())
            {
                var dict = DictionaryHelper.ToOrderedDictionary(
                    obj.GetType().GetProperties(BindingFlags),
                    a => a.Name.Humanize(LetterCasing.Title),
                    a => Get(valueConverter, obj, a)?.ToString() ?? throw new Exception("FSDs r"));

                yield return KeyValuePair.Create(obj.GetType().Name.Humanize(LetterCasing.Title), dict);
            }
        }

        private static string? Get(IValueConverter<(PropertyInfo, object), string?> valueConverter, object value, PropertyInfo memberInfo)
        {
            return valueConverter.Convert((memberInfo, value), null);
        }

        private static PropertyInfo[] SelectMembers(IReflect type, IComparer<string>? comparer = null)
        {
            PropertyInfo[] members =
                type.GetProperties(BindingFlags).ToArray();
            return comparer != null ? members
                    .OrderBy(a => a.Name, comparer).ToArray() :
                members;
        }

        private static Dictionary<string, PropertyInfo> SelectMembersAsDictionary(IReflect type, IComparer<string>? comparer = null)
        {
            return SelectMembers(type, comparer).ToDictionary(a => a.Name, a => a);
        }


    }
}