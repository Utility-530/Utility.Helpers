using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilityHelper.NonGeneric;


namespace UtilityHelper
{



    public static class AttributeHelper
    {


        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetDescription(this Type value)
        {


            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])value.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }




        public static IEnumerable<Type> FilterByCategoryAttribute(this Type[] types, string category)
        {

            return types.Where(_ =>
            {
                var ca = _.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault();
                return ca == null ? false :
                ((CategoryAttribute)ca).Category.Equals(category, StringComparison.OrdinalIgnoreCase);
            });


        }


        public static IEnumerable<KeyValuePair<string, Type>> ToKeyValuePairs(IEnumerable<Type> types)
        {

            return types.Select(_ => new KeyValuePair<string, Type>(_.ToString(), _));

        }




    }

}
