using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace UtilityHelper
{
    public static class AttributeHelper
    {
        public static string GetDescriptionSafe(object value) => GetAttributeSafe<DescriptionAttribute>(value, a => a.Description);

        public static string GetAttributeSafe<T>(object value, Func<T, string> func) where T : Attribute => value.GetType().GetAttributeSafe(func);

        public static string GetDescriptionSafe(this MemberInfo type) => type.GetAttributeSafe<DescriptionAttribute>(_ => _.Description);

        public static string GetDescription(object value) => GetAttribute<DescriptionAttribute>(value, a => a.Description);

        public static string GetAttribute<T>(object value, Func<T, string> func) where T : Attribute => value.GetType().GetAttribute(func);

        public static string GetDescription(this MemberInfo type) => type.GetAttribute<DescriptionAttribute>(_ => _.Description);

        public static string GetAttribute<T>(this MemberInfo value, Func<T, string> func) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            if (attributes?.Length > 0)
                return func(attributes[0]);
            else
                throw new Exception($"Member has not attributes of type {typeof(T).Name}");
        }

        public static string GetAttributeSafe<T>(this MemberInfo value, Func<T, string> func) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            if (attributes?.Length > 0)
                return func(attributes[0]);
            else
                return value.ToString();
        }

        public static string GetAttributeSafe(this MemberInfo value, Func<object, string> func, Type type)
        {
            object[] attributes = value.GetCustomAttributes(type, false);

            if (attributes?.Length > 0)
                return func(attributes[0]);
            else
                return value.ToString();
        }

        public static IEnumerable<Type> FilterByCategoryAttribute(this System.Type[] types, string category) =>
            types.Where(type =>
            {
                var ca = type.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault();
                return ca == null ? false :
                ((CategoryAttribute)ca).Category.Equals(category, StringComparison.OrdinalIgnoreCase);
            });
    }
}