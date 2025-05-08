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

        public static IEnumerable<(string, Func<object?>)> MethodValues(this object instance, params object[] parameters)
        {
            return instance.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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

        public static string AsString(this MethodInfo mi)
        {
            StringBuilder sb = new();
            // Get method body information.
            //MethodInfo mi = typeof(Example).GetMethod("MethodBodyExample");
            MethodBody mb = mi.GetMethodBody();
            sb.AppendLine($"Method: {mi}");

            // Display the general information included in the
            // MethodBody object.
            sb.AppendLine($"Local variables are initialized: {mb.InitLocals}");
            sb.AppendLine($"Maximum number of items on the operand stack: {mb.MaxStackSize}");

            // Display information about the local variables in the
            // method body.
            sb.AppendLine();
            foreach (LocalVariableInfo lvi in mb.LocalVariables)
            {
                sb.AppendLine($"Local variable: {lvi}");
            }

            // Display exception handling clauses.
            sb.AppendLine();
            foreach (ExceptionHandlingClause ehc in mb.ExceptionHandlingClauses)
            {
                sb.AppendLine(ehc.Flags.ToString());

                // The FilterOffset property is meaningful only for Filter
                // clauses. The CatchType property is not meaningful for
                // Filter or Finally clauses.
                switch (ehc.Flags)
                {
                    case ExceptionHandlingClauseOptions.Filter:
                        sb.AppendLine($"Filter Offset: {ehc.FilterOffset}"
                            );
                        break;

                    case ExceptionHandlingClauseOptions.Finally:
                        break;

                    default:
                        sb.AppendLine($"Type of exception: {ehc.CatchType}");
                        break;
                }

                sb.AppendLine($"Handler Length: {ehc.HandlerLength}");
                sb.AppendLine($"Handler Offset: {ehc.HandlerOffset}");
                sb.AppendLine($"Try Block Length: {ehc.TryLength}");
                sb.AppendLine($"Try Block Offset: {ehc.TryOffset}");
            }
            return sb.ToString();
        }

        // This code example produces output similar to the following:
        //
        //Method: Void MethodBodyExample(System.Object)
        //    Local variables are initialized: True
        //    Maximum number of items on the operand stack: 2
        //
        //Local variable: System.Int32 (0)
        //Local variable: System.String (1)
        //Local variable: System.Exception (2)
        //Local variable: System.Boolean (3)
        //
        //Filter
        //      Filter Offset: 71
        //      Handler Length: 23
        //      Handler Offset: 116
        //      Try Block Length: 61
        //      Try Block Offset: 10
        //Clause
        //    Type of exception: System.Exception
        //       Handler Length: 21
        //       Handler Offset: 70
        //     Try Block Length: 61
        //     Try Block Offset: 9
        //Finally
        //       Handler Length: 14
        //       Handler Offset: 94
        //     Try Block Length: 85
        //     Try Block Offset: 9
    }
}
