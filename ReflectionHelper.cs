using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class ReflectionHelper
    {

        public static Object GetPropValue(this Object obj, String name)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        public static T GetPropValue<T>(this Object obj, String name)
        {
            Object retval = GetPropValue(obj, name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }




   


     
    }




    //Stack Overflow Dustin Campbell answered Oct 27 '10 at 17:58
    public static class TypeExtensions
    {
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        //Stack Overflow nawfal Oct 9 '13 at 7:44

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
           return  type.GetMethods()
                .FirstOrDefault(m =>
                m.Name == name &&
                m.GetParameters()
                .Select(p => p.ParameterType)
                .SequenceEqual(parameterTypes, new SimpleTypeComparer()));
        }
    }
}
