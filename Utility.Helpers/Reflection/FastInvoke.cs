using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Utility.Helpers.Reflection
{
    /// <summary>
    /// <a href="https://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142"></a>
    /// </summary>
    public static class FastInvoke
    {
        public static Func<object, TParameter> ToGetter<TParameter>(this MemberInfo memberInfo)
        {
            return ToGetter<object, TParameter>(memberInfo);
        }

        public static Func<TInstance, TParameter> ToGetter<TInstance, TParameter>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            var declaringType = memberInfo.DeclaringType;
            var instanceParam = Expression.Parameter(typeof(TInstance), "obj");

            // Cast obj to the declaring type
            var castInstance = Expression.Convert(instanceParam, declaringType);
            // Access the member
            var memberAccess = Expression.MakeMemberAccess(castInstance, memberInfo);
            // Convert the result to the desired output type
            var convertResult = Expression.Convert(memberAccess, typeof(TParameter));

            // Compile lambda
            var lambda = Expression.Lambda<Func<TInstance, TParameter>>(convertResult, instanceParam);
            return lambda.Compile();
        }

        public static Action<TInstance, object> ToSetter<TInstance>(this MemberInfo memberInfo)
        {
            return ToSetter<TInstance, object>(memberInfo);
        }

        public static Action<TInstance, TParameter> ToSetter<TInstance, TParameter>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            var instanceType = typeof(TInstance);
            var targetType = memberInfo.DeclaringType;
            var valueType = UnderlyingType(memberInfo);

            var instanceParam = Expression.Parameter(instanceType, "instance");
            var valueParam = Expression.Parameter(typeof(TParameter), "value");

            // Ensure instance is casted if needed (e.g., TInstance is base of declaring type)
            var castedInstance = instanceType == targetType
                ? (Expression)instanceParam
                : Expression.Convert(instanceParam, targetType);

            // Cast value (object) to the actual member type
            var convertedValue = Expression.Convert(valueParam, valueType);

            var memberAccess = Expression.MakeMemberAccess(castedInstance, memberInfo);
            var assignExpr = Expression.Assign(memberAccess, convertedValue);

            var lambda = Expression.Lambda<Action<TInstance, TParameter>>(assignExpr, instanceParam, valueParam);
            return lambda.Compile();
        }

        public static Type UnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;

                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;

                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;

                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;

                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        // Create an fill objects fast from DataReader
        // http://flurfunk.sdx-ag.de/2012/05/c-performance-bei-der-befullungmapping.html
        public static IEnumerable<T> CreateObjectFromReader<T>(IDataReader reader, Func<IDataReader, List<string>> getFieldNames)
            where T : new()
        {
            // Prepare
            List<string> fieldNames = getFieldNames(reader);
            List<Action<T, object>> setterList = [];
            List<T> result = new();

            // Create Property-Setter and store it in an array
            foreach (var field in fieldNames)
            {
                var propertyInfo = typeof(T).GetProperty(field);
                setterList.Add(FastInvoke.ToSetter<T>(propertyInfo));
            }
            Action<T, object>[] setterArray = setterList.ToArray();

            // generate and fill objects
            while (reader.Read())
            {
                T xclass = new T();
                int fieldNumber = 0;

                for (int i = 0; i < setterArray.Length; i++)
                {
                    // call setter
                    setterArray[i](xclass, reader.GetValue(i));
                    fieldNumber++;
                }
                //result.Add(xclass);
                yield return xclass;
            }
        }
    }
}