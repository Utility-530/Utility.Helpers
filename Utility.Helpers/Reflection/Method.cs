using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utility.Helpers.Reflection
{
    public record struct PropResult(string Name, object Value, Type Type);

    public static class MethodHelpers
    {

        private static Dictionary<object, Delegate> _cache4GetExpressionValueAsMethodCallExpression = new Dictionary<object, Delegate>();
        private static Dictionary<object, Delegate> _cache4GetExpressionValueAsUnaryExpression = new Dictionary<object, Delegate>();
        private static Dictionary<Type, Dictionary<PropertyInfo, Delegate>> _cache4FastGetters = new Dictionary<Type, Dictionary<PropertyInfo, Delegate>>();

        public static PropResult GetExpressionValue<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> exp)
        {
            Type type = typeof(TSource);

            if (exp.Body is MethodCallExpression)
            {
                var member = exp.Body as MethodCallExpression;
                //var methodResult = compiledLambda.DynamicInvoke(source);
                var methodResult = _cache4GetExpressionValueAsMethodCallExpression.Get(exp, e => Expression.Lambda(member, exp.Parameters.ToArray()).Compile()).DynamicInvoke(source);
                return new PropResult(member.ToString(), methodResult, source.GetType());
            }

            if (exp.Body is UnaryExpression)
            {
                var member = exp.Body as UnaryExpression;
                //var methodResult = compiledLambda.DynamicInvoke(source);
                var methodResult = _cache4GetExpressionValueAsUnaryExpression.Get(exp, e => Expression.Lambda(member, exp.Parameters.ToArray()).Compile()).DynamicInvoke(source);
                return new PropResult(member.ToString(), methodResult, source.GetType()); ;
            }

            if (exp.Body is MemberExpression)
            {
                var member = exp.Body as MemberExpression;
                var propInfo = member.Member as PropertyInfo;
                //methodResult = propInfo.GetValue(source, null);
                var value = _cache4FastGetters.Get(type, _ => new()).Get(propInfo, p => p.ToGetter<TSource>()).DynamicInvoke(source);
                return new PropResult(propInfo.Name, value, source.GetType());
            }

            throw new NotImplementedException("GetExpressionValue");
        }

        public static IEnumerable<(string, Func<object?>)> StaticMethodValues(this Type t, params object[] parameters)
        {
            return t
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(null, parameters))));
        }

        public static IEnumerable<(string, MethodInfo)> StaticMethods(this Type t)
        {
            return t
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Select(m => (m.GetDescription(), m));
        }

        public static IEnumerable<MethodInfo> InstantMethods(this Type instance)
        {
            return instance
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName);

        }


        public static IEnumerable<(string, Func<object?>)> MethodValues(this object instance, params object[] parameters)
        {
            return instance.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(instance, parameters))));
        }


        public static string Description(this MethodInfo methodInfo)
        {
            try
            {
                object[] attribArray = methodInfo.GetCustomAttributes(false);

                if (attribArray.Length > 0)
                {
                    if (attribArray[0] is DescriptionAttribute attrib)
                        return attrib.Description;
                }
            }
            catch (NullReferenceException)
            {
            }
            return methodInfo.Name;
        }


        /// <summary>
        /// Converts a MethodInfo to a string representation for lookup purposes
        /// Format: TypeName.MethodName(ParameterType1,ParameterType2,...)
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to convert</param>
        /// <returns>String representation of the method</returns>
        public static string AsString(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return "null";

            var sb = new StringBuilder();

            // Type name (full name for uniqueness)
            sb.Append(methodInfo.DeclaringType?.FullName ?? "Unknown");
            sb.Append(".");
            sb.Append(methodInfo.Name);
            sb.Append("(");

            // Parameter types
            var parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i].ParameterType.FullName);
                if (i < parameters.Length - 1)
                    sb.Append(",");
            }

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Converts a string back to a MethodInfo using reflection
        /// String format: TypeName.MethodName(ParameterType1,ParameterType2,...)
        /// </summary>
        /// <param name="methodString">String representation of the method</param>
        /// <returns>MethodInfo object or null if not found</returns>
        public static MethodInfo AsMethodInfo(this string methodString)
        {
            if (string.IsNullOrEmpty(methodString) || methodString == "null")
                return null;

            try
            {
                // Parse the string format: TypeName.MethodName(param1,param2,...)
                int lastDotIndex = methodString.LastIndexOf('.', methodString.IndexOf('('));
                if (lastDotIndex == -1)
                    throw new ArgumentException("Invalid method string format");

                string typeName = methodString.Substring(0, lastDotIndex);
                string methodPart = methodString.Substring(lastDotIndex + 1);

                int parenIndex = methodPart.IndexOf('(');
                if (parenIndex == -1)
                    throw new ArgumentException("Invalid method string format");

                string methodName = methodPart.Substring(0, parenIndex);
                string paramsPart = methodPart.Substring(parenIndex + 1);
                paramsPart = paramsPart.TrimEnd(')');

                // Get the type
                Type declaringType = Type.GetType(typeName);
                if (declaringType == null)
                {
                    // Try to find in loaded assemblies
                    declaringType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.FullName == typeName);
                }

                if (declaringType == null)
                    throw new TypeLoadException($"Could not load type: {typeName}");

                // Parse parameter types
                Type[] parameterTypes;
                if (string.IsNullOrEmpty(paramsPart))
                {
                    parameterTypes = new Type[0];
                }
                else
                {
                    string[] paramTypeNames = paramsPart.Split(',');
                    parameterTypes = new Type[paramTypeNames.Length];

                    for (int i = 0; i < paramTypeNames.Length; i++)
                    {
                        string paramTypeName = paramTypeNames[i].Trim();
                        Type paramType = Type.GetType(paramTypeName);
                        if (paramType == null)
                        {
                            // Try to find in loaded assemblies
                            paramType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.FullName == paramTypeName);
                        }

                        if (paramType == null)
                            throw new TypeLoadException($"Could not load parameter type: {paramTypeName}");

                        parameterTypes[i] = paramType;
                    }
                }

                // Get the method
                MethodInfo method = declaringType.GetMethod(methodName, parameterTypes);
                return method;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to parse method string: {methodString}", ex);
            }
        }
    }
}

