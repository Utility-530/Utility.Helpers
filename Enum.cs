using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UtilityHelper
{
    public static class EnumHelper
    {
        public static T MatchByName<T>(Enum r) where T : struct
        {
            var name = r.ToString().ToLowerInvariant().Remove("_");
            return Enum.GetValues(typeof(T)).Cast<T>().SingleOrDefault(t => t.ToString().ToLowerInvariant().Remove("_").Equals(name));
        }

        public static IEnumerable<(T one, R two)> JoinByName<T, R>()
            where T : struct
            where R : struct
        {
            var func = new Func<T, string>(t => t.ToString().ToLowerInvariant().Remove("_"));
            var func2 = new Func<R, string>(r => r.ToString().ToLowerInvariant().Remove("_"));

            return LinqExtension.FullOuterJoin(Enum.GetValues(typeof(T)).Cast<T>(), Enum.GetValues(typeof(R)).Cast<R>(), func, func2);
        }

        public static T ToEnum<T>(int i) => (T)Enum.ToObject(typeof(T), i);

        public static T GetValueFromDescription<T>(string description, StringComparison stringcomparison)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description.Equals(description, stringcomparison))
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (attribute.Description.Equals(field.Name, stringcomparison))
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        public static T Parse<T>(string value) => (T)Enum.Parse(typeof(T), value, true);

        public static object ParseByReflection(Type type, string value, string[] names = null)
        {
            return Enum.ToObject(type, (names ?? Enum.GetNames(type)).Select((a, i) => new { a, i }).SingleOrDefault(c => c.a == value).i);
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static T GetAttribute<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static string GetDescription(this Enum e, bool toStringIfNone = true)
        {
            var fi = e.GetType().GetField(e.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((attributes.Length > 0) ? attributes[0].Description : toStringIfNone ? e.ToString() : null);
        }

        public static IEnumerable<KeyValuePair<string, int>> GetAllValuesAndDescriptions(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T is not System.Enum");

            foreach (var e in Enum.GetValues(enumType))
                yield return new KeyValuePair<string, int>(GetDescription((Enum)e), (int)e);
        }

        public static IEnumerable<KeyValuePair<string, T>> GetAllValuesAndDescriptions<T>()
        {
            var enumType = typeof(T);
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T is not System.Enum");

            foreach (var e in Enum.GetValues(enumType))
                yield return new KeyValuePair<string, T>(GetDescription((Enum)e), (T)e);
        }

        public static IEnumerable<string> GetAllDescriptions(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T is not System.Enum");

            foreach (var e in Enum.GetValues(enumType))
                yield return GetDescription((Enum)e);
        }
    }

    public static class EnumCycler<T> where T : Enum
    {
        private static readonly Lazy<T[]> enums = new Lazy<T[]>(() => Enum.GetValues(typeof(T)).Cast<T>().ToArray());

        public static T Next(T side) => (T)(object)((Convert.ToByte(side) + 1) % (Convert.ToByte(enums.Value.Last()) + 1) + Convert.ToByte(enums.Value.First()));
    }
}