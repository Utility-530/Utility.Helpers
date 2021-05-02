using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace UtilityHelper
{
    public static class AttributeHelper
    {
        public static string GetDescriptionSafe(object value) => GetAttributeStringPropertySafe<DescriptionAttribute>(value, a => a.Description);

        public static string GetAttributeStringPropertySafe<T>(object value, Func<T, string> func) where T : Attribute => value.GetType().GetAttributeStringPropertySafe(func);

        public static string GetDescriptionSafe(this MemberInfo type) => type.GetAttributeStringPropertySafe<DescriptionAttribute>(attribute => attribute.Description);

        public static string GetDescription(object value) => GetAttributeProperty<DescriptionAttribute, string>(value, a => a.Description);

        public static TR GetAttributeProperty<T, TR>(object value, Func<T, TR> func) where T : Attribute => value.GetType().GetAttributeProperty(func);

        public static string GetDescription(this MemberInfo type) => type.GetAttributeProperty<DescriptionAttribute, string>(a => a.Description);

        public static string GetAttributeProperty<T>(this MemberInfo value, Func<T, string> func) where T : Attribute => GetAttributeProperty<T, string>(value, func);

        public static TR GetAttributeProperty<T, TR>(this MemberInfo value, Func<T, TR> func) where T : Attribute => func(GetAttribute<T>(value));

        public static T GetAttribute<T>(this MemberInfo value) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            if (attributes?.Length > 0)
                return attributes[0];
            throw new Exception($"Member has not attributes of type {typeof(T).Name}");
        }

        public static (bool success, T attribute) GetAttributeSafe<T>(this MemberInfo value) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            return attributes?.Length > 0 ? (true, attributes[0]) : (false, default)!;
        }     
        

        public static (bool success, object? attribute) GetAttributeSafe(this MemberInfo value, Type type)
        {
            object[] attributes = value.GetCustomAttributes(type, false);

            return attributes?.Length > 0 ? (true, attributes[0]) : (false, default)!;
        }

        public static object GetAttribute(this MemberInfo value, Type type)
        {
            object[] attributes = value.GetCustomAttributes(type, false);

            if (attributes?.Length > 0)
                return attributes[0];
            throw new Exception($"Member has not attributes of type {type.Name}");
        }

        public static string GetAttributeStringPropertySafe<T>(this MemberInfo value, Func<T, string> func) where T : Attribute
        {
            return GetAttributeSafe<T>(value) is (bool success, T t) && success ? func(t) : value.ToString();
        }

        public static string GetAttributeStringPropertySafe(this MemberInfo value, Func<object, string> func, Type type)
        {
            return GetAttributeSafe(value, type) is (bool success, object t) && success ? func(t) : value.ToString();
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
