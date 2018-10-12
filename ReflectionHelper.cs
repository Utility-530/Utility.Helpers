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
    public static class PropertyHelper
    {
        public static T GetPropValue<T>(this Object obj, String name, Type type = null) => GetPropValue<T>(obj, (type ?? obj.GetType()).GetProperty(name));

        public static object GetPropValue(this Object obj, String name, Type type = null) => GetPropValue(obj, (type ?? obj.GetType()).GetProperty(name));

        public static T GetPropValue<T, R>(R obj, String name) => GetPropValue<T>(obj, typeof(R).GetProperty(name));

        public static object GetPropValue(this Object obj, PropertyInfo info = null)
        {
            if (info == null) return null;
            object retval = info.GetValue(obj, null);
            return retval == null ? null : retval;
        }


        public static T GetPropValue<T>(this Object obj, PropertyInfo info = null)
        {
            if (info == null) return default(T);
            object retval = info.GetValue(obj, null);
            return retval == null ? default(T) : (T)retval;
        }


        public static IEnumerable<T> GetPropValues<T>(this IEnumerable<Object> obj, String name, Type type = null)
        {
            var x = (type ?? obj.First().GetType()).GetProperty(name);
            return obj.Select(_ => GetPropValue<T>(_, x));
        }

        public static IEnumerable<T> GetPropValues<T, R>(IEnumerable<R> obj, String name)
        {
            var x = typeof(R).GetProperty(name);
            return obj.Select(_ => GetPropValue<T>(_, x));
        }

        public static IEnumerable<T> GetPropValues<T>(this IEnumerable<Object> obj, PropertyInfo info = null) => obj.Select(_ => GetPropValue<T>(_, info));


        public static IEnumerable<T> GetPropValues<T>(this IEnumerable obj, PropertyInfo info = null)
        {
            foreach (var x in obj)
                yield return GetPropValue<T>(x, info);
        }

        public static IEnumerable<T> GetPropValues<T>(this IEnumerable obj, String name, Type type = null)
        {
            var info = (type ?? obj.First().GetType()).GetProperty(name);
            foreach (var x in obj)
                yield return GetPropValue<T>(x, info);
        }

        public static IEnumerable GetPropValues(this IEnumerable obj, String name, Type type = null)
        {
            var info = (type ?? obj.First().GetType()).GetProperty(name);
            foreach (var x in obj)
                yield return GetPropValue(x, info);
        }



        public static bool SetPropertyByType<T>(object obj, T value)
        {
            var properties = obj.GetType().GetProperties();
            var prop = properties.SingleOrDefault(_ => _.PropertyType == typeof(T));
            if (prop != null)
            {
                prop.SetValue(obj, value, null);
                return true;
            }
            return false;
        }


        public static void SetValue(object inputObject, string propertyName, object propertyVal, bool ignoreCase = true)
        {
            System.Reflection.PropertyInfo propertyInfo = null;
            //get the property information based on the 
            if (ignoreCase)
                propertyInfo = inputObject.GetType().GetProperty(propertyName, BindingFlags.SetProperty |
                       BindingFlags.IgnoreCase |
                       BindingFlags.Public |
                       BindingFlags.Instance);
            else
                propertyInfo = inputObject.GetType().GetProperty(propertyName);

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyInfo.PropertyType) ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);

        }


        private static bool IsNullableType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));


        public static T ToObject<T>(this Dictionary<string, object> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                //  type.GetProperty(kv.Key).SetValue(obj, kv.Value);

                if (kv.Value != null)

                    PropertyHelper.SetValue(obj, kv.Key, kv.Value);


            }
            return (T)obj;
        }



        public static T ToObject<T>(this Dictionary<string, string> dict, Dictionary<string, Type> propertytypes = null)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            propertytypes = propertytypes ?? type.GetProperties().ToDictionary(_ => _.Name, _ => _.PropertyType);


            foreach (var kv in dict)
            {
                //  type.GetProperty(kv.Key).SetValue(obj, kv.Value);

                if (kv.Value != null)

                    PropertyHelper.SetValue(obj, kv.Key, Convert.ChangeType(kv.Value, propertytypes[kv.Key]));
            }
            return (T)obj;
        }


        public static object ToObject(this Dictionary<string, string> dict, Type type, Dictionary<string, Type> propertytypes = null)
        {
 
            var obj = Activator.CreateInstance(type);

            propertytypes = propertytypes ?? type.GetProperties().ToDictionary(_ => _.Name, _ => _.PropertyType);


            foreach (var kv in dict)
            {
                //  type.GetProperty(kv.Key).SetValue(obj, kv.Value);

                if (kv.Value != null)

                    PropertyHelper.SetValue(obj, kv.Key, Convert.ChangeType(kv.Value, propertytypes[kv.Key]));
            }
            return obj;
        }


        public static IEnumerable<T> ToObjects<T>(this IEnumerable<Dictionary<string, string>> dicts)
        {

            Dictionary<string, Type> propertytypes = typeof(T).GetProperties().ToDictionary(_ => _.Name, _ => _.PropertyType);

            foreach (var dict in dicts)
                yield return dict.ToObject<T>(propertytypes);

        }



        public static double[] ObjectToDoubleArray(object myobject, params string[] excludeProperties)
        {
            return
         myobject.GetType()
             .GetProperties()
             .Where(p => (!excludeProperties.Contains(p.Name) &&
                p.PropertyType.IsNumerical()))
            .Select(p => Convert.ToDouble(p.GetValue(myobject))).ToArray();
        }

        public static double[][] ObjectsToDoubleArray(IEnumerable<object> objects, params string[] excludeProperties)
        {
            var props = objects.GetType()
        .GetProperties()
        .Where(p => (!excludeProperties.Contains(p.Name)))
         .Where(p => p.PropertyType.IsNumerical());

            return objects.Select(_ => props.Select(p => Convert.ToDouble(p.GetValue(objects))).ToArray()).ToArray();
        }



        public static bool IsNumericType(this object o)
        {
            return o.GetType().IsNumerical();
        }

        public static bool IsNumerical(this Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }






    public static class TypeExtensions
    {



        //Stack Overflow nawfal Oct 9 '13 at 7:44
        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            return type.GetMethods()
                 .FirstOrDefault(m =>
                 m.Name == name &&
                 m.GetParameters()
                 .Select(p => p.ParameterType)
                 .SequenceEqual(parameterTypes, new SimpleTypeComparer()));
        }

        //Stack Overflow Dustin Campbell answered Oct 27 '10 at 17:58
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


        public static IEnumerable<KeyValuePair<string, Func<T>>> LoadMethods<T>(this Type t, params object[] parameters)
        {
            return Assembly.GetAssembly(t)
                  .GetType(t.FullName)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                       // filter by return type
                       .Where(a => a.ReturnType.Name == typeof(T).Name)
                        .Select(_ => new KeyValuePair<string, Func<T>>(_.Name, () => (T)_.Invoke(null, parameters)));

        }


        public static IEnumerable<KeyValuePair<string, Action>> ToActions<T>(this IEnumerable<KeyValuePair<string, Func<T>>> kvps, Action<T> tr)
        {
            foreach (var m in kvps)
            {
                yield return new KeyValuePair<string, Action>(
                      m.Key,
                    () => tr(m.Value())

                );
            }
        }

        public static Func<T, R> GetInstanceMethod<T, R>(MethodInfo method)
        {
            //ParameterExpression x = Expression.Parameter(typeof(T), "it");
            return Expression.Lambda<Func<T, R>>(
                Expression.Call(null, method), null).Compile();
        }



        public static IEnumerable<Type> GetInheritingTypes(Type type)
        {
            var x = AssemblyHelper.GetSolutionAssemblies();

            var sf = x.SelectMany(sd => sd.GetExportedTypes());

            var v = sf.Where(p => p.GetInterfaces().Any(t => t == type));

            return v;

        }




        public static Type[] GetTypesInNamespace(this Assembly assembly, string nameSpace)
            => assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();



        //        Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "MyNamespace");
        //for (int i = 0; i<typelist.Length; i++)
        //{
        //    Console.WriteLine(typelist[i].Name);
        //}







    }



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



    public static class AssemblyHelper
    {

        public static IEnumerable<Assembly> GetSolutionAssemblies()
        {
            var list = new HashSet<string>();
            var stack = new Stack<Assembly>();

            stack.Push(Assembly.GetEntryAssembly());

            while (stack.Count > 0)
            {
                var asm = stack.Pop();

                yield return asm;

                foreach (var reference in asm.GetReferencedAssemblies())
                    if (!reference.FullName.Contains("Deedle"))
                        if (list.Add(reference.FullName))
                            stack.Push(Assembly.Load(reference));



            }


        }

    }
}

