using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utility.Helpers.NonGeneric;

namespace Utility.Helpers
{
    public class PropertyCache<R> where R : notnull
    {
        private readonly Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>();

        public PropertyCache()
        {
            dictionary = typeof(R).GetProperties().ToDictionary(a => a.Name, a => a);
        }

        public T? GetPropertyValue<T>(R obj, string name) where T : struct => PropertyHelper.GetPropertyValue<T>(obj, dictionary[name]);

        public T? GetPropertyRefValue<T>(R obj, string name) where T : class => PropertyHelper.GetPropertyRefValue<T>(obj, dictionary[name]);

        public IEnumerable<string> GetValues<T>(R obj) => dictionary.Select(a => a.Value.GetValue(obj).ToString());

        public IEnumerable<T?> GetPropertyValues<T>(IEnumerable<R> obj, string name) where T : struct => obj.Select(r => GetPropertyValue<T>(r, name));

        public IEnumerable<T?> GetPropertyRefValues<T>(IEnumerable<R> obj, string name) where T : class => obj.Select(r => GetPropertyRefValue<T>(r, name));

        public string[] PropertyNames => dictionary.Keys.Cast<string>().ToArray();
    }

    public static class PropertyHelper
    {
        public static T? GetPropertyValue<T>(this object obj, string name, Type? type = null) where T : struct => GetPropertyValue<T>(obj, (type ?? obj.GetType()).GetProperty(name));

        public static T? GetPropertyRefValue<T>(this object obj, string name, Type? type = null) where T : class => GetPropertyRefValue<T>(obj, (type ?? obj.GetType()).GetProperty(name));

        public static T? GetPropertyValue<T, R>(R obj, string name) where T : struct where R : notnull => GetPropertyValue<T>(obj, typeof(R).GetProperty(name));

        public static T? GetPropertyRefValue<T, R>(R obj, string name) where T : class where R : notnull => GetPropertyRefValue<T>(obj, typeof(R).GetProperty(name));

        public static T? GetPropertyValue<T>(this object obj, PropertyInfo? info = null) where T : struct
        {
            if (info == null)
                return default;
            object retval = info.GetValue(obj, null);
            return retval == null ? default : (T)retval;
        }

        public static T? GetPropertyRefValue<T>(this object obj, PropertyInfo? info = null) where T : class
        {
            if (info == null)
                return default;
            object retval = info.GetValue(obj, null);
            return retval == null ? default : (T)retval;
        }

        public static IEnumerable<T?> GetPropertyRefValues<T, R>(IEnumerable<R> obj, string name) where T : class where R : notnull
        {
            var x = typeof(R).GetProperty(name);
            return obj.Select(a => GetPropertyRefValue<T>(a, x));
        }

        public static IEnumerable<T?> GetPropertyValues<T, R>(IEnumerable<R> obj, string name) where T : struct where R : notnull
        {
            var x = typeof(R).GetProperty(name);
            return obj.Select(a => GetPropertyValue<T>(a, x));
        }

        public static IEnumerable<T?> GetPropertyValues<T>(this IEnumerable<object> obj, PropertyInfo? info = null) where T : struct => obj.Select(a => GetPropertyValue<T>(a, info));

        public static IEnumerable<T?> GetPropertyRefValues<T>(this IEnumerable<object> obj, PropertyInfo? info = null) where T : class => obj.Select(a => GetPropertyRefValue<T>(a, info));

        public static IEnumerable<T?> GetPropertyValues<T>(this IEnumerable obj, PropertyInfo? info = null) where T : class
        {
            foreach (var x in obj)
                yield return GetPropertyRefValue<T>(x, info);
        }

        public static IEnumerable<T?> GetPropertyRefValues<T>(this IEnumerable obj, PropertyInfo? info = null) where T : struct
        {
            foreach (var x in obj)
                yield return GetPropertyValue<T>(x, info);
        }

        public static IEnumerable<T?> GetPropertyValues<T>(this IEnumerable obj, string name, Type? type = null) where T : struct
        {
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                foreach (var x in obj)
                    yield return (T)Convert.ChangeType((x as IDictionary)![name], t);
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                foreach (var x in obj)
                {
                    if (x is System.Data.DataRow dataRow)
                        if (!IsCastableTo(dataRow[name].GetType(), t))
                        {
                            var response = TryChangeType(dataRow[name], t);
                            if (response.IsSuccess)
                            {
                                yield return (T?)response.Value;
                            }
                            else
                                yield return (T)Convert.ChangeType(dataRow[name], t); ;
                        }
                        else
                            yield return (T)(dataRow[name]);
                }
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                foreach (var x in obj)
                    yield return GetPropertyValue<T>(x, info);
            }
        }

        public static IEnumerable<T?> GetPropertyRefValues<T>(this IEnumerable obj, string name, Type? type = null) where T : class
        {
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                foreach (var x in obj)
                    yield return (T)Convert.ChangeType((x as IDictionary)![name], t);
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                foreach (var x in obj)
                {
                    if (x is System.Data.DataRow dataRow)
                        if (!IsCastableTo(dataRow[name].GetType(), t))
                        {
                            var response = TryChangeType(dataRow[name], t);
                            if (response.IsSuccess)
                            {
                                yield return (T?)response.Value;
                            }
                            else
                                yield return (T)Convert.ChangeType(dataRow[name], t); ;
                        }
                        else
                            yield return (T)(dataRow[name]);
                }
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                foreach (var x in obj)
                    yield return GetPropertyRefValue<T>(x, info);
            }
        }

        public static T? GetPropertyValueSafe<T>(object x, string name) where T : class
        {
            var type = x.GetType();

            var t = typeof(T);
            if (type == typeof(System.Data.DataRow))
            {
                System.Data.DataRow dataRow = (x as System.Data.DataRow)!;
                if (!IsCastableTo(dataRow[name].GetType(), t))
                {
                    var response = TryChangeType(dataRow[name], t);
                    if (response.IsSuccess)
                    {
                        return (T?)response.Value;
                    }
                    else
                        return (T)Convert.ChangeType(dataRow[name], t);
                }
                else
                    return (T)(dataRow[name]);
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                return GetPropertyRefValue<T>(x, info);
            }
        }

        public static IEnumerable<T?> GetPropertyValuesSafe<T>(this IEnumerable obj, string name, Type? type = null) where T : struct
        {
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                foreach (var x in obj)
                    yield return (T)Convert.ChangeType((x as IDictionary)![name], t);
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                foreach (var x in obj)
                {
                    if (x is System.Data.DataRow dataRow)
                    {
                        if (!IsCastableTo(dataRow[name].GetType(), t))
                        {
                            var response = TryChangeType(dataRow[name], t);
                            if (response.IsSuccess)
                            {
                                yield return (T?)response.Value;
                            }
                            else
                                yield return (T)Convert.ChangeType(dataRow[name], t); ;
                        }
                        else
                            yield return (T)(dataRow)[name];
                    }
                    //(T)Convert.ChangeType(, );
                }
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                foreach (var x in obj)
                    yield return PropertyHelper.GetPropertyValue<T>(x, info);
            }
        }

        public static IEnumerable<T?> GetPropertyRefValuesSafe<T>(this IEnumerable obj, string name, Type? type = null) where T : class
        {
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                foreach (var x in obj)
                    yield return (T)Convert.ChangeType((x as IDictionary)![name], t);
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                foreach (var x in obj)
                {
                    if (x is System.Data.DataRow dataRow)
                    {
                        if (!IsCastableTo(dataRow[name].GetType(), t))
                        {
                            var response = TryChangeType(dataRow[name], t);
                            if (response.IsSuccess)
                            {
                                yield return (T?)response.Value;
                            }
                            else
                                yield return (T)Convert.ChangeType(dataRow[name], t); ;
                        }
                        else
                            yield return (T)(dataRow)[name];
                    }
                    //(T)Convert.ChangeType(, );
                }
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                foreach (var x in obj)
                    yield return PropertyHelper.GetPropertyRefValue<T>(x, info);
            }
        }

        public static IEnumerable<Dictionary<string, object?>> GetPropertyValues(this IEnumerable obj, Dictionary<string, Type> propnames, Type? type = null)
        {
            var dataRow = obj.First() as System.Data.DataRow;
            if (dataRow == null)
                yield break;

            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                foreach (var x in obj)
                    yield return propnames
                        .ToDictionary(
                        name => name.Key,
                        name => (object?)Convert.ChangeType((x as IDictionary)![name.Key], name.Value));
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var xx = propnames.ToDictionary(name => name, name =>
                {
                    if (!PropertyHelper.IsCastableTo(dataRow[name.Key].GetType(), name.Value))
                        return (PropertyHelper.TryChangeType(dataRow[name.Key], name.Value).IsSuccess) ? 1 : 2;
                    else
                        return 3;
                });

                foreach (var dr in obj.OfType<System.Data.DataRow>())
                {
                    yield return xx.ToDictionary(name => name.Key.Key, name =>
                    {
                        return name.Value switch
                        {
                            (1) => PropertyHelper.TryChangeType(dr[name.Key.Key], name.Key.Value).Value,
                            (2) => Convert.ChangeType(dr[name.Key.Key], name.Key.Value),
                            (3) => dr[name.Key.Key],
                            _ => null,
                        };
                    });
                }
            }
            else
            {
                var xx = propnames.ToDictionary(name => name.Key, name => (type).GetProperty(name.Key));

                foreach (var x in obj)
                    yield return xx.ToDictionary(name => name.Key, name => GetPropertyRefValue<object>(x, name.Value));
            }
        }

        // https://stackoverflow.com/questions/1399273/test-if-convert-changetype-will-work-between-two-types
        // answered Dec 8 '17 at 16:46Immac
        public static object? ChangeType(object value, Type conversion)
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
        public static (bool IsSuccess, object? Value) TryChangeType(object value, Type conversionType)
        {
            (bool IsSuccess, object? Value) response = (false, null);
            var isNotConvertible = !(value is IConvertible) || !(value.GetType() == conversionType);

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
        public static Type[] ValueTypes = new[]{
             typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(ulong),
        typeof(long),
        typeof(uint),
        typeof(int),
        typeof(ushort),
                typeof(short), typeof(byte), typeof(char), typeof(bool)};

        public static Type[] NumberTypes = new[]{
             typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(ulong),
        typeof(long),
        typeof(uint),
        typeof(int),
        typeof(ushort),
                typeof(short), typeof(byte)};

        private static Dictionary<Type, List<Type>> dict = new Dictionary<Type, List<Type>>() {
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

        public static IEnumerable<T?> GetPropertyValues<T>(this IEnumerable<object> obj, string name, Type? type = null) where T : struct
        {
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                return obj.Select(x => (T?)Convert.ChangeType((x as IDictionary)![name], t));
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                return obj.Select(x => (T?)Convert.ChangeType((x as System.Data.DataRow)![name], t));
            }
            else
                return GetPropertyValues<T>(obj, (type).GetProperty(name));
        }

        public static IEnumerable<T?> GetPropertyRefValues<T>(this IEnumerable<object> obj, string name, Type? type = null) where T : class
        {
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                var t = typeof(T);
                return obj.Select(x => (T)Convert.ChangeType((x as IDictionary)![name], t));
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var t = typeof(T);
                return obj.Select(x => (T)Convert.ChangeType((x as System.Data.DataRow)![name], t));
            }
            else
                return GetPropertyRefValues<T>(obj, (type).GetProperty(name));
        }

        public static bool SetPropertyByType<T>(object obj, T value)
        {
            var properties = obj.GetType().GetProperties();
            var prop = properties.SingleOrDefault(a => a.PropertyType == typeof(T));
            if (prop != null)
            {
                prop.SetValue(obj, value, null);
                return true;
            }
            return false;
        }

        public static void SetValue(object inputObject, string propertyName, object propertyVal, bool ignoreCase = true)
        {
            PropertyInfo? propertyInfo;
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

            if (targetType.IsAssignableFrom(propertyVal.GetType()) == false)
                propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);
        }

        public static bool IsNullableType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

        public static T Map<T>(this Dictionary<string, object> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict.Where(a => a.Value != null))
            {
                SetValue(obj, kv.Key, kv.Value);
            }
            return (T)obj;
        }

        public static T Map<T>(this Dictionary<string, string> dict, Dictionary<string, Type>? propertytypes = null)
        {
            return (T)MapToObject(dict, typeof(T), propertytypes);
        }

        public static object MapToObject(this Dictionary<string, string> dict, Type type, Dictionary<string, Type>? propertytypes = null)
        {
            var obj = Activator.CreateInstance(type);

            propertytypes ??= type.GetProperties().ToDictionary(a => a.Name, a => a.PropertyType);

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

        public static IEnumerable<T> MapToMany<T>(this IEnumerable<Dictionary<string, string>> dicts)
        {
            Dictionary<string, Type> propertytypes = typeof(T).GetProperties().ToDictionary(a => a.Name, a => a.PropertyType);

            foreach (var dict in dicts)
                yield return dict.Map<T>(propertytypes);
        }

        public static double[] GetDoubleArray(object myobject, params string[] excludeProperties)
        {
            return
         myobject.GetType()
             .GetProperties()
             .Where(p => (!excludeProperties.Contains(p.Name) &&
                p.PropertyType.IsNumerical()))
            .Select(p => Convert.ToDouble(p.GetValue(myobject))).ToArray();
        }

        public static double[][] GetDoubleArrays(IEnumerable<object> objects, params string[] excludeProperties)
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

        public static IEnumerable<PropertyInfo> GetNullProperties<T, R>(R myObject, bool isNull, IEnumerable<PropertyInfo> propertyInfos) => from property in propertyInfos
                                                                                                                                             let value = property.GetValue(myObject)
                                                                                                                                             where (value == null) == isNull
                                                                                                                                             select property;

        /// <summary>
        /// How to check all properties of an object are either null or empty?
        /// <see href="https://stackoverflow.com/questions/22683040/how-to-check-all-properties-of-an-object-whether-null-or-empty"/> Matthew Watson
        /// </summary>
        /// <param name="myObject"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetNullProperties<T, R>(R myObject, bool isNull) => GetNullProperties<T, R>(myObject, isNull, from property in typeof(R).GetProperties()
                                                                                                                                              where property.PropertyType == typeof(T)
                                                                                                                                              select property);

        public static bool IsAnyPropertyNull<T, R>(R myObject, bool isNull) => GetNullProperties<T, R>(myObject, isNull).Any();

        public static bool IsAnyPropertyNull<T, R>(R myObject, bool isNull, IEnumerable<PropertyInfo> propertyInfos) => GetNullProperties<T, R>(myObject, isNull, propertyInfos).Any();

        public static IEnumerable<R> SelectWhereNoPropertiesNull<T, R>(IEnumerable<R> myObjects, bool isNull)
        {
            PropertyInfo[] propertyInfos = (from property in typeof(R).GetProperties()
                                            where property.PropertyType == typeof(T)
                                            select property).ToArray();

            return from myObject in myObjects
                   where IsAnyPropertyNull<T, R>(myObject, isNull, propertyInfos)
                   select myObject;
        }

        /// <summary>
        /// How to check all properties of an object are either null or empty?
        /// <see href="https://stackoverflow.com/questions/22683040/how-to-check-all-properties-of-an-object-whether-null-or-empty"/> Matthew Watson
        /// </summary>
        /// <param name="myObject"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPropertiesByPredicate<T, R>(R myObject, Predicate<T> predicate) => GetPropertiesByPredicate<T, R>(myObject, predicate, from property in typeof(R).GetProperties()
                                                                                                                                                                          where property.PropertyType == typeof(T)
                                                                                                                                                                          select property);

        public static bool IsTrue<T, R>(R myObject, Predicate<T> predicate) => GetPropertiesByPredicate<T, R>(myObject, predicate).Any();

        public static bool IsTrue<T, R>(R myObject, Predicate<T> predicate, IEnumerable<PropertyInfo> propertyInfos) => GetPropertiesByPredicate<T, R>(myObject, predicate, propertyInfos).Any();

        public static IEnumerable<R> SelectByPredicateAny<T, R>(IEnumerable<R> myObjects, Predicate<T> predicate)
        {
            PropertyInfo[] propertyInfos = (from property in typeof(R).GetProperties()
                                            where property.PropertyType == typeof(T)
                                            select property).ToArray();

            return from myObject in myObjects
                   where GetPropertiesByPredicate(myObject, predicate, propertyInfos).Any()
                   select myObject;
        }

        public static IEnumerable<R> SelectByPredicateAll<T, R>(IEnumerable<R> myObjects, Predicate<T> predicate)
        {
            PropertyInfo[] propertyInfos = (from property in typeof(R).GetProperties()
                                            where property.PropertyType == typeof(T)
                                            select property).ToArray();

            return from myObject in myObjects
                   where propertyInfos.Count() == (GetPropertiesByPredicate<T, R>(myObject, predicate, propertyInfos).Count())
                   select myObject;
        }

        public static IEnumerable<PropertyInfo> GetPropertiesByPredicate<T, R>(
            R myObject,
            Predicate<T> predicate,
            IEnumerable<PropertyInfo> propertyInfos) => from property in propertyInfos
                                                        let value = (T)property.GetValue(myObject)
                                                        where predicate(value)
                                                        select property;
    }
}