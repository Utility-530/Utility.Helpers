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

        public static string GetDescription(Enum value) => value.GetType().GetField(value.ToString()).GetKey<DescriptionAttribute>(_ => _.Description);

        public static string GetDescription(this MemberInfo type) => type.GetKey<DescriptionAttribute>(_ => _.Description);


        public static string GetKey<T>(this MemberInfo value, Func<T, string> func) where T : Attribute
        {
            T[] attributes = (T[])value.GetCustomAttributes(typeof(T), false);

            if (attributes?.Length > 0)
                return func(attributes[0]);
            else
                return value.ToString();
        }


        public static IEnumerable<Type> FilterByCategoryAttribute(this System.Type[] types, string category) => 
            types.Where(_ =>
            {
                var ca = _.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault();
                return ca == null ? false :
                ((CategoryAttribute)ca).Category.Equals(category, StringComparison.OrdinalIgnoreCase);
            });

    }
}
