using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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
                    if (attribute?.Description.Equals(field.Name, stringcomparison) ?? false)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        public static T Parse<T>(string value) => (T)Enum.Parse(typeof(T), value, true);

        public static object ParseByReflection(Type type, string value, string[]? names = null)
        {
            return Enum.ToObject(type, (names ?? Enum.GetNames(type)).Select((a, i) => new { a, i }).SingleOrDefault(c => c.a == value).i);
        }

        public static string? GetDescription(this Enum e, bool toStringIfNone = true)
        {
            var fi = e.GetType().GetField(e.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((attributes.Length > 0) ? attributes[0].Description : toStringIfNone ? e.ToString() : null);
        }

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException($"{nameof(type)} must be an enum type");

            return Enum.GetValues(type).Cast<Enum>()
                .Where(e => e.GetAttribute<BrowsableAttribute>()?.Browsable ?? true)
                .Select(e =>
                new ValueDescription
                {
                    Value = e,
                    Description = e.GetDescription(type) ?? e.ToString().Replace("_", " ")
                }).ToList();
        }

        public static IEnumerable<ValueDescription<T>> SelectAllValuesAndDescriptions<T>() where T : Enum
        {
            return SelectAllValuesAndDescriptions<T>(typeof(T));
        }

        public static IEnumerable<ValueDescription<T>> SelectAllValuesAndDescriptions<T>(this Type type) where T : Enum
        {
            return Enum.GetValues(type).Cast<T>()
                .Where(e => e.GetAttribute<BrowsableAttribute>()?.Browsable ?? true)
                .Select(e => new ValueDescription<T>(e.GetDescription(type) ?? e.ToString().Replace("_", " "), e))
                .ToList();
        }

        public static IEnumerable<string?> GetAllDescriptions(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T is not System.Enum");

            foreach (var e in Enum.GetValues(enumType))
                yield return GetDescription((Enum)e);
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// <see href="https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value"/>
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T? GetAttribute<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        public static bool IsFlagSet<T>(this T value, T flag) where T : Enum
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public static IEnumerable<T> GetFlags<T>(this T value) where T : Enum
        {
            return from enm in Enum.GetValues(typeof(T)).Cast<T>()
                   where value.IsFlagSet(enm)
                   select enm;
        }

        public static T SetFlags<T>(this T value, T flags, bool on) where T : Enum
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flags);
            if (on)
            {
                lValue |= lFlag;
            }
            else
            {
                lValue &= (~lFlag);
            }
            return (T)Enum.ToObject(typeof(T), lValue);
        }

        public static T SetFlags<T>(this T value, T flags) where T : Enum => value.SetFlags(flags, true);

        public static T ClearFlags<T>(this T value, T flags) where T : Enum => value.SetFlags(flags, false);

        public static T CombineFlags<T>(this IEnumerable<T> flags) where T : Enum
        {
            long lValue = flags
                .Select(flag => Convert.ToInt64(flag))
                .Aggregate<long, long>(0, (current, lFlag) => current | lFlag);

            return (T)Enum.ToObject(typeof(T), lValue);
        }

        public static bool IsEqualToDefault(this Enum value)
        {
            var type = value.GetType();
            var first = type.GetFields(BindingFlags.Static | BindingFlags.Public).First().GetValue(null);
            return value.Equals(first);
        }

        public static TAttribute? GetAttribute<T, TAttribute>(this T value)
            where T : Enum
            where TAttribute : Attribute
        {
            return GetAttribute<T, TAttribute>(value, typeof(T));
        }

        public static TAttribute? GetAttribute<T, TAttribute>(this T value, Type type)
            where T : Enum
            where TAttribute : Attribute
        {
            if (Enum.GetName(type, value) is { } name &&
                type.GetField(name) is { } field &&
                Attribute.GetCustomAttribute(field, typeof(TAttribute)) is TAttribute attribute)
                return attribute;
            return null;
        }

        public static string? GetDescription<T>(this T value) where T : Enum
        {
            return GetDescription(value, typeof(T));
        }

        public static string? GetDescription<T>(this T value, Type type) where T : Enum
        {
            return GetAttribute<T, DescriptionAttribute>(value, type)?.Description;
        }

        public class ValueDescription
        {
            public string? Description { get; set; }
            public Enum? Value { get; set; }
        }

        public class ValueDescription<T> where T : Enum
        {
            public ValueDescription(string description, T value)
            {
                Description = description;
                Value = value;
            }

            public string Description { get; set; }
            public T Value { get; set; }
        }
    }

    public static class EnumCycler<T> where T : Enum
    {
        private static readonly Lazy<T[]> enums = new Lazy<T[]>(() => Enum.GetValues(typeof(T)).Cast<T>().ToArray());

        public static T Next(T side) => (T)(object)((Convert.ToByte(side) + 1) % (Convert.ToByte(enums.Value.Last()) + 1) + Convert.ToByte(enums.Value.First()));
    }
}