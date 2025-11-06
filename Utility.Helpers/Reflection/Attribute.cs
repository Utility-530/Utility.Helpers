using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Utility.Helpers.Reflection
{
    public static class AttributeHelper
    {
        public static string GetDescriptionSafe(object value) => GetAttributeStringPropertySafe<DescriptionAttribute>(value, a => a.Description);

        public static string GetAttributeStringPropertySafe<T>(object value, Func<T, string> func) where T : Attribute => value.GetType().GetAttributeStringPropertySafe(func);

        public static string GetDescriptionSafe(this MemberInfo type) => type.GetAttributeStringPropertySafe<DescriptionAttribute>(attribute => attribute.Description);

        public static string GetDescription(object value) => GetAttributeProperty<DescriptionAttribute, string>(value, a => a.Description);

        public static TR GetAttributeProperty<T, TR>(object value, Func<T, TR> func) where T : Attribute => value.GetType().GetAttributeProperty(func);

        public static string GetDescription(this MemberInfo type) => type.GetAttributeProperty<DescriptionAttribute, string>(a => a.Description);

        public static string GetAttributeProperty<T>(this MemberInfo value, Func<T, string> func) where T : Attribute => value.GetAttributeProperty<T, string>(func);

        public static TR GetAttributeProperty<T, TR>(this MemberInfo value, Func<T, TR> func) where T : Attribute => func(value.GetAttribute<T>());

        public static T GetAttribute<T>(this MemberInfo value) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            if (attributes?.Length > 0)
                return attributes[0];
            throw new Exception($"Member has not attributes of type {typeof(T).Name}");
        }

        public static (bool success, T? attribute) GetAttributeSafe<T>(this MemberInfo value) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            return attributes?.Length > 0 ? (true, attributes[0]) : (false, default)!;
        }
        
        public static bool TryGetAttribute<T>(this MemberInfo value, out T? attribute) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);
            attribute = attributes.FirstOrDefault();
            return attributes?.Length > 0;
        }
        public static bool HasAttribute<T>(this MemberInfo value) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);
            return attributes?.Length > 0;
        }

        public static TR? GetAttributePropertySafe<T, TR>(this MemberInfo value, Func<T, TR> propertyFunc) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            return attributes?.Length > 0 ? propertyFunc(attributes[0]) : default;
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
            return value.GetAttributeSafe<T>() is (bool success, T t) && success ? func(t) : value.ToString();
        }

        public static string GetAttributeStringPropertySafe(this MemberInfo value, Func<object, string> func, Type type)
        {
            return value.GetAttributeSafe(type) is (bool success, object t) && success ? func(t) : value.ToString();
        }

        public static IEnumerable<Type> FilterByCategoryAttribute(this Type[] types, string category) =>
            types.Where(type =>
            {
                var ca = type.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault();
                return ca == null ? false :
                ((CategoryAttribute)ca).Category.Equals(category, StringComparison.OrdinalIgnoreCase);
            });

        public static PropertyInfo[] FilterPropertiesByAttribute<TAttribute>(Type type)
            where TAttribute : Attribute => FilterPropertiesByAttribute(type, typeof(TAttribute));

        public static PropertyInfo[] FilterPropertiesByAttribute(Type type, Type AttributeType)          
        {
            return [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => Attribute.IsDefined(prop, AttributeType))];
        }

        public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            if (provider == null)
            {
                return null;
            }

            object[] o = provider.GetCustomAttributes(typeof(T), true);
            if (o == null || o.Length == 0)
            {
                return null;
            }

            return (T)o[0];
        }

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static T GetAttribute<T>(this MemberDescriptor descriptor) where T : Attribute
        {
            if (descriptor == null)
            {
                return null;
            }

            return descriptor.Attributes.GetAttribute<T>();
        }

        public static T GetAttribute<T>(this AttributeCollection attributes) where T : Attribute
        {
            if (attributes == null)
            {
                return null;
            }

            foreach (object att in attributes)
            {
                if (typeof(T).IsAssignableFrom(att.GetType()))
                {
                    return (T)att;
                }
            }
            return null;
        }

        public static IEnumerable<T> GetAttributes<T>(this MemberInfo element) where T : Attribute
        {
            return (IEnumerable<T>)Attribute.GetCustomAttributes(element, typeof(T));
        }

    }
}