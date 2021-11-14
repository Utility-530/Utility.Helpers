using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UtilityHelper.NonGeneric;

namespace UtilityHelper
{
    public class PropertyCache<R>
    {
        private Type type;
        private Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>();

        public PropertyCache()
        {
            type = typeof(R);
            dictionary = typeof(R).GetProperties().ToDictionary(a => a.Name, a => a);
        }

        public T GetPropertyValue<T>(R obj, string name) => UtilityHelper.PropertyHelper.GetPropertyValue<T>(obj, dictionary[name]);

        public IEnumerable<string> GetValues<T>(R obj) => dictionary.Select(a => a.Value.GetValue(obj).ToString());

        public IEnumerable<T> GetPropertyValues<T>(IEnumerable<R> obj, string name) => obj.Select(r => GetPropertyValue<T>(r, name));

        public string[] PropertyNames => dictionary.Keys.Cast<string>().ToArray();
    }

    public static class PropertyHelper
    {
        public static T GetPropertyValue<T>(this object obj, string name, Type type = null) => GetPropertyValue<T>(obj, (type ?? obj.GetType()).GetProperty(name));

        public static T GetPropertyValue<T, R>(R obj, string name) => GetPropertyValue<T>(obj, typeof(R).GetProperty(name));

        public static T? GetPropertyValue<T>(this object obj, PropertyInfo? info = null)
        {
            return (T?)info?.GetValue(obj, null);
        }

        public static IEnumerable<T> GetPropertyValues<T, R>(IEnumerable<R> obj, string name)
        {
            var x = typeof(R).GetProperty(name);
            return obj.Select(_ => GetPropertyValue<T>(_, x));
        }

        public static IEnumerable<T> GetPropertyValues<T>(this IEnumerable<object> obj, PropertyInfo info = null) => obj.Select(_ => GetPropertyValue<T>(_, info));

        public static IEnumerable<T> GetPropertyValues<T>(this IEnumerable obj, PropertyInfo info = null)
        {
            foreach (var x in obj)
                yield return GetPropertyValue<T>(x, info);
        }

        public static IEnumerable<T> GetPropertyValues<T>(this IEnumerable obj, string name, Type type = null)
        {
            type ??= obj.First().GetType();

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
                    yield return GetPropertyValue<T>(x, info);
            }
        }

        public static T GetPropertyValueSafe<T>(object x, string name)
        {
            var type = x.GetType();

            var t = typeof(T);
            if (type == typeof(System.Data.DataRow))
            {
                if (!PropertyHelper.IsCastableTo((x as System.Data.DataRow)[name].GetType(), t))
                {
                    var response = PropertyHelper.TryChangeType((x as System.Data.DataRow)[name], t);
                    if (response.IsSuccess)
                    {
                        return (T)response.Value;
                    }
                    else
                        return (T)Convert.ChangeType((x as System.Data.DataRow)[name], t);
                }
                else
                    return (T)((x as System.Data.DataRow)[name]);
            }
            else
            {
                PropertyInfo info = (type).GetProperty(name);
                return PropertyHelper.GetPropertyValue<T>(x, info);
            }
        }

        public static IEnumerable<T> GetPropertyValuesSafe<T>(this IEnumerable obj, String name, Type type = null)
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
                    if (!PropertyHelper.IsCastableTo((x as System.Data.DataRow)[name].GetType(), t))
                    {
                        var response = PropertyHelper.TryChangeType((x as System.Data.DataRow)[name], t);
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
                    yield return PropertyHelper.GetPropertyValue<T>(x, info);
            }
        }

        public static IEnumerable<Dictionary<string, object>> GetPropertyValues(this IEnumerable obj, Dictionary<string, Type>? propnames=default, Type? type = null)
        {
            var xs = obj.First();
            type ??= obj.First().GetType();

            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                foreach (var x in obj)
                    yield return propnames
                        .ToDictionary(
                        name => name.Key,
                        name => Convert.ChangeType((x as IDictionary)[name.Key], name.Value));
            }
            else if (type == typeof(System.Data.DataRow))
            {
                var xx = propnames.ToDictionary(name => name, name =>
                {
                    System.Data.DataRow dr = (xs as System.Data.DataRow);
                    if (!PropertyHelper.IsCastableTo(dr[name.Key].GetType(), name.Value))
                        return (PropertyHelper.TryChangeType(dr[name.Key], name.Value).IsSuccess) ? 1 : 2;
                    else
                        return 3;
                });

                foreach (var x in obj)
                {
                    yield return xx.ToDictionary(name => name.Key.Key, name =>
                    {
                        System.Data.DataRow dr = (x as System.Data.DataRow);
                        switch (name.Value)
                        {
                            case (1):
                                return PropertyHelper.TryChangeType(dr[name.Key.Key], name.Key.Value).Value;

                            case (2):
                                return Convert.ChangeType(dr[name.Key.Key], name.Key.Value);

                            case (3):
                                return dr[name.Key.Key];

                            default:
                                return null;
                        }
                    });
                }
            }
            else
            {
                var xx = propnames.ToDictionary(name => name.Key, name => (type).GetProperty(name.Key));
                foreach (var x in obj)
                    yield return xx.ToDictionary(name => name.Key, name => PropertyHelper.GetPropertyValue<object>(x, name.Value));
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

        public static IEnumerable<T> GetPropertyValues<T>(this IEnumerable<object> obj, string name, Type type = null)
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
                return GetPropertyValues<T>(obj, (type).GetProperty(name));
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

        public static T Map<T>(this Dictionary<string, string> dict, Dictionary<string, Type> propertytypes = null)
        {
            return (T)MapToObject(dict, typeof(T), propertytypes);
        }

        public static object MapToObject(this Dictionary<string, string> dict, Type type, Dictionary<string, Type> propertytypes = null)
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

        public static IEnumerable<T> MapToMany<T>(this IEnumerable<Dictionary<string, string>> dicts)
        {
            Dictionary<string, Type> propertytypes = typeof(T).GetProperties().ToDictionary(_ => _.Name, _ => _.PropertyType);

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


    public static class PocoToDictionary
    {
        private static readonly MethodInfo AddToDictionaryMethod = typeof(IDictionary<string, object>).GetMethod("Add");
        private static readonly ConcurrentDictionary<Type, Func<object, IDictionary<string, object>>> Converters = new ConcurrentDictionary<Type, Func<object, IDictionary<string, object>>>();
        private static readonly ConstructorInfo DictionaryConstructor = typeof(Dictionary<string, object>).GetConstructors().FirstOrDefault(c => c.IsPublic && !c.GetParameters().Any());

        /// <summary>
        /// <a href="https://softwareproduction.eu/2018/02/28/fast-conversion-objects-dictionaries-c/"></a>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object obj) => obj == null ? null : Converters.GetOrAdd(obj.GetType(), o =>
        {
            var outputType = typeof(IDictionary<string, object>);
            var inputType = obj.GetType();
            var inputExpression = Expression.Parameter(typeof(object), "input");
            var typedInputExpression = Expression.Convert(inputExpression, inputType);
            var outputVariable = Expression.Variable(outputType, "output");
            var returnTarget = Expression.Label(outputType);
            var body = new List<Expression>
        {
            Expression.Assign(outputVariable, Expression.New(DictionaryConstructor))
        };
            body.AddRange(
                from prop in inputType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                where prop.CanRead && (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                let getExpression = Expression.Property(typedInputExpression, prop.GetMethod)
                let convertExpression = Expression.Convert(getExpression, typeof(object))
                select Expression.Call(outputVariable, AddToDictionaryMethod, Expression.Constant(prop.Name), convertExpression));
            body.Add(Expression.Return(returnTarget, outputVariable));
            body.Add(Expression.Label(returnTarget, Expression.Constant(null, outputType)));

            var lambdaExpression = Expression.Lambda<Func<object, IDictionary<string, object>>>(
                Expression.Block(new[] { outputVariable }, body),
                inputExpression);

            return lambdaExpression.Compile();
        })(obj);


        public static Dictionary<string, object> ToDictionary2(object source)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            string[] keys = { };
            object[] values = { };

            bool outLoopingKeys = false, outLoopingValues = false;

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (value is Dictionary<string, object>.KeyCollection)
                {
                    keys = ((Dictionary<string, object>.KeyCollection)value).ToArray();
                    outLoopingKeys = true;
                }
                if (value is Dictionary<string, object>.ValueCollection)
                {
                    values = ((Dictionary<string, object>.ValueCollection)value).ToArray();
                    outLoopingValues = true;
                }
                if (outLoopingKeys & outLoopingValues)
                {
                    break;
                }
            }

            for (int i = 0; i < keys.Length; i++)
            {
                result.Add(keys[i], values[i]);
            }

            return result;
        }
    }
}