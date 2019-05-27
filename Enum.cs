using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{

    public static class EnumHelper
    {

        // answered Sep 28 '10 at 20:23 Dirk Vollmar
        // This gets you a plain array of the enum values using Enum.GetValues:

        //var valuesAsArray = Enum.GetValues(typeof(Enumnum));

        // And this gets you a generic list:

        // var valuesAsList = Enum.GetValues(typeof(Enumnum)).Cast<Enumnum>().ToList();


     


        public static T ToEnum<T>(int i)
        {

            return (T)Enum.ToObject(typeof(T), i);

        }


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


        public static T Parse<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static object ParseByReflection(Type type, string value, string[] names = null)
        {
            names = names ?? Enum.GetNames(type);
            return Enum.ToObject(type, names.Select((a, i) => new { a, i }).SingleOrDefault(c => c.a == value).i);
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




        public static string GetDescription(this Enum e)
        {

            var fi = e.GetType().GetField(e.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((attributes.Length > 0) ? attributes[0].Description : e.ToString());

        }




        public static IEnumerable<KeyValuePair<string, int>> GetAllValuesAndDescriptions(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T is not System.Enum");

            List<KeyValuePair<string, int>> enumValList = new List<KeyValuePair<string, int>>();

            foreach (var e in Enum.GetValues(enumType))
                yield return new KeyValuePair<string, int>(GetDescription((Enum)e), (int)e);


        }



        public static IEnumerable<string> GetAllDescriptions(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T is not System.Enum");

            foreach (var e in Enum.GetValues(enumType))
                yield return GetDescription((Enum)e);


        }




    }


}
