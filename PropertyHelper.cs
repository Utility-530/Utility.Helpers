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
    public static partial class PropertyHelper
    {
        public static T GetPropValue<T>(this Object obj, String name, Type type = null) => GetPropValue<T>(obj, (type ?? obj.GetType()).GetProperty(name));


        public static T GetPropValue<T, R>(R obj, String name) => GetPropValue<T>(obj, typeof(R).GetProperty(name));


        public static T GetPropValue<T>(this Object obj, PropertyInfo info = null)
        {
            if (info == null) return default(T);
            object retval = info.GetValue(obj, null);
            return retval == null ? default(T) : (T)retval;
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
            type = type ?? obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                foreach (var x in obj)
                    yield return (T)Convert.ChangeType((x as IDictionary)[name], t);
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                foreach (var x in obj)
                {
                    if (!IsCastableTo((x as System.Data.DataRow)[name].GetType(), t))
                    {
                        var response = TryChangeType((x as System.Data.DataRow)[name], t);
                        if (response.IsSuccess)
                        {
                            yield return (T)response.Value;
                        }
                        else
                            yield return (T)Convert.ChangeType((x as System.Data.DataRow)[name], t); ;
                    }
                    else
                        yield return (T)((x as System.Data.DataRow)[name]);
                    //(T)Convert.ChangeType(, );
                }
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                foreach (var x in obj)
                    yield return GetPropValue<T>(x, info);
            }
        }

        // https://stackoverflow.com/questions/1399273/test-if-convert-changetype-will-work-between-two-types
        // answered Dec 8 '17 at 16:46Immac
        public static object ChangeType(object value, Type conversion)
        {
            var type = conversion;

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                type = Nullable.GetUnderlyingType(type);
            }

            return Convert.ChangeType(value, type);
        }

        //https://stackoverflow.com/questions/1399273/test-if-convert-changetype-will-work-between-two-types
        // answered Dec 8 '17 at 16:46Immac
        public static (bool IsSuccess, object Value) TryChangeType(object value, Type conversionType)
        {
            (bool IsSuccess, object Value) response = (false, null);
            var isNotConvertible =
                conversionType == null
                    || value == null
                    || !(value is IConvertible)
                || !(value.GetType() == conversionType);
            if (isNotConvertible)
            {
                return response;
            }
            try
            {
                response = (true, ChangeType(value, conversionType));
            }
            catch (Exception)
            {
                response.Value = null;
            }

            return response;
        }
        static Dictionary<Type, List<Type>> dict = new Dictionary<Type, List<Type>>() {
             { typeof(decimal), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char) } },
        { typeof(double), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float) } },
        { typeof(float), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float) } },
        { typeof(ulong), new List<Type> { typeof(byte), typeof(ushort), typeof(uint), typeof(char) } },
        { typeof(long), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(char) } },
        { typeof(uint), new List<Type> { typeof(byte), typeof(ushort), typeof(char) } },
        { typeof(int), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(char) } },
        { typeof(ushort), new List<Type> { typeof(byte), typeof(char) } },
        { typeof(short), new List<Type> { typeof(byte) } }
    };

        //https://stackoverflow.com/questions/2224266/how-to-tell-if-type-a-is-implicitly-convertible-to-type-b
        // answered Feb 8 '10 at 19:51        jason
        public static bool IsCastableTo(this Type from, Type to)
        {
            if (to.IsAssignableFrom(from))
            {
                return true;
            }
            if (dict.ContainsKey(to) && dict[to].Contains(from))
            {
                return true;
            }
            bool castable = from.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Any(
                                m => m.ReturnType == to &&
                                (m.Name == "op_Implicit" ||
                                m.Name == "op_Explicit")
                            );
            return castable;
        }

        //below is a more robust alternative 
        //https://stackoverflow.com/questions/2224266/how-to-tell-if-type-a-is-implicitly-convertible-to-type-b
        //public static bool IsImplicitlyCastableTo(this Type from, Type to)
        //{
        //    // from http://www.codeducky.org/10-utilities-c-developers-should-know-part-one/ 
        //    Throw.IfNull(from, "from");
        //    Throw.IfNull(to, "to");

        //    // not strictly necessary, but speeds things up
        //    if (to.IsAssignableFrom(from))
        //    {
        //        return true;
        //    }

        //    try
        //    {
        //        // overload of GetMethod() from http://www.codeducky.org/10-utilities-c-developers-should-know-part-two/ 
        //        // that takes Expression<Action>
        //        ReflectionHelpers.GetMethod(() => AttemptImplicitCast<object, object>())
        //            .GetGenericMethodDefinition()
        //            .MakeGenericMethod(from, to)
        //            .Invoke(null, new object[0]);
        //        return true;
        //    }
        //    catch (TargetInvocationException ex)
        //    {
        //        return  !(
        //            ex.InnerException is RuntimeBinderException
        //            // if the code runs in an environment where this message is localized, we could attempt a known failure first and base the regex on it's message
        //            && System.Text.RegularExpressions.Regex.IsMatch(ex.InnerException.Message, @"^The best overloaded method match for 'System.Collections.Generic.List<.*>.Add(.*)' has some invalid arguments$")
        //        );
        //    }
        //}



        public static IEnumerable<T> GetPropValues<T>(this IEnumerable<Object> obj, String name, Type type = null)
        {
            type = type ?? obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                return obj.Select(x => (T)Convert.ChangeType((x as IDictionary)[name], t));
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                return obj.Select(x => (T)Convert.ChangeType((x as System.Data.DataRow)[name], t));
            }
            else
                return GetPropValues<T>(obj, (type).GetProperty(name));

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
            return (T)ToObject(dict, typeof(T), propertytypes);
        }


        public static object ToObject(this Dictionary<string, string> dict, Type type, Dictionary<string, Type> propertytypes = null)
        {
            var obj = Activator.CreateInstance(type);

            propertytypes = propertytypes ?? type.GetProperties().ToDictionary(_ => _.Name, _ => _.PropertyType);

            foreach (var kv in dict)
            {
                if (kv.Value != null)
                    if (propertytypes[kv.Key].IsEnum)
                        PropertyHelper.SetValue(obj, kv.Key, EnumHelper.ParseByReflection(propertytypes[kv.Key], kv.Value));
                    else
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









}

