#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Utility
{
    public static class EnumHelper
    {
        static readonly Lazy<Random> random = new(() => new Random());

        public static T Random<T>() where T : Enum
        {
            var arr = Enum.GetValues(typeof(T));
            return (T)(arr.GetValue(random.Value.Next(0, arr.Length - 1)) ?? throw new NullReferenceException("sdf sdfe 44"));
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

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException($"{nameof(type)} must be an enum type");

            return Enum.GetValues(type).Cast<Enum>()
                .Where(e => e.GetAttribute<BrowsableAttribute>()?.Browsable ?? true)
                .Select(e => new ValueDescription(e, e.GetDescription(type) ?? e.ToString().Replace("_", " ")))
                .ToList();
        }

        public static IEnumerable<ValueDescription<T>> GetAllValuesAndDescriptions<T>() where T : Enum
        {
            var type = typeof(T);
            return Enum.GetValues(type).Cast<T>()
                .Where(e => e.GetAttribute<BrowsableAttribute>()?.Browsable ?? true)
                .Select(e =>
                new ValueDescription<T>(e, e.GetDescription(type) ?? e.ToString().Replace("_", " "))
              ).ToList();
        }
    }

    public record ValueDescription(Enum? Value, string? Description);


    public record ValueDescription<T> : ValueDescription where T : Enum
    {
        public ValueDescription(T Value, string Description) : base(Value, Description)
        {
        }
    }
}