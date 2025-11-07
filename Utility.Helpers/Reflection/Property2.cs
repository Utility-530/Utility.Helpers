using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Utility.Helpers
{
    /// <summary>
    /// <a href="https://stackoverflow.com/questions/5031288/i-need-to-access-a-non-public-member-highlighted-item-of-a-combo-box"/>
    /// </summary>
    public static class PropertyHelper2
    {
        //public static PropertyDescriptor GetPropertyDescriptor(PropertyInfo PropertyInfo)
        //{
        //    return TypeDescriptor.GetProperties(PropertyInfo.DeclaringType).Item(PropertyInfo.Name);
        //}
        public static PropertyDescriptor ToPropertyDescriptor(this PropertyInfo propertyInfo)
        {
            return TypeDescriptor.GetProperties(propertyInfo.DeclaringType)[propertyInfo.Name];
        }

        public static PropertyDescriptor? PropertyDescriptor(this object target, [CallerMemberName] string propertyName = "") =>
            TypeDescriptor.GetProperties(target.GetType()).Find(propertyName, ignoreCase: false);

        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivatePropertyValue<T>(this object obj, string propName)
        {
            return (T)obj.GetPrivatePropertyValue(propName);
        }

        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static object GetPrivatePropertyValue(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            PropertyInfo pi = obj.GetType().GetProperty(propName,
                                                        BindingFlags.Public | BindingFlags.NonPublic |
                                                        BindingFlags.Instance) ?? throw new ArgumentOutOfRangeException("propName",
                                                      string.Format("Property {0} was not found in Type {1}", propName,
                                                                    obj.GetType().FullName));
            return pi.GetValue(obj, null);
        }

        /// <summary>
        /// Returns a private Field Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Field</typeparam>
        /// <param name="obj">Object from where the Field Value is returned</param>
        /// <param name="propName">Field Name as string.</param>
        /// <returns>FieldValue</returns>
        public static T GetPrivateFieldValue<T>(this object obj, string propName)
        {
            return (T)obj.GetPrivateFieldValue(propName);
        }

        /// <summary>
        /// Returns a private Field Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Field</typeparam>
        /// <param name="obj">Object from where the Field Value is returned</param>
        /// <param name="propName">Field Name as string.</param>
        /// <returns>FieldValue</returns>
        public static object? GetPrivateFieldValue(this object obj, string propName)
        {
            if (obj.TryGetPrivateFieldValue(propName, out var value))
                return value;
            else
                throw new ArgumentOutOfRangeException("propName",
                                               string.Format("Field {0} was not found in Type {1}", propName,
                                                             obj.GetType().FullName));
        }

        public static bool TryGetPrivateFieldValue(this object obj, string propName, out object? output)
        {
            return TryGetPrivateFieldValue(obj, propName, out output, out _);
        }

        public static bool TryGetPrivateFieldValue(this object obj, string propName, out object? output, out FieldInfo? fieldInfo)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            fieldInfo = null;
            while (fieldInfo == null && t != null)
            {
                fieldInfo = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fieldInfo == null)
            {
                output = default;
                return false;
            }
            output = fieldInfo.GetValue(obj);
            return true;
        }

        /// <summary>
        /// Sets a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is set</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">Value to set.</param>
        /// <returns>PropertyValue</returns>
        public static void SetPrivatePropertyValue<T>(this object obj, string propName, T val)
        {
            Type t = obj.GetType();
            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
                throw new ArgumentOutOfRangeException("propName",
                                                      string.Format("Property {0} was not found in Type {1}", propName,
                                                                    obj.GetType().FullName));
            t.InvokeMember(propName,
                           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty |
                           BindingFlags.Instance, null, obj, new object[] { val });
        }

        /// <summary>
        /// Set a private Field Value on a given Object. Uses Reflection.
        /// </summary>
        /// <typeparam name="T">Type of the Field</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Field name as string.</param>
        /// <param name="val">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        public static bool SetPrivateFieldValue<T>(this object obj, string fieldName, T val)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (val == null) throw new ArgumentNullException("value is null");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                t = t.BaseType;
            }
            if (fi == null)
                throw new ArgumentOutOfRangeException("propName",
                                                      string.Format("Field {0} was not found in Type {1}", fieldName,
                                                                    obj.GetType().FullName));
            if (fi.GetValue(obj)?.Equals(val) != true)
            {
                fi.SetValue(obj, val);
                return true;
            }
            return false;
        }
    }
}