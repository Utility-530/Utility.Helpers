using Utility.Interfaces.Methods;

namespace Utility.Extensions
{
    public static class MethodExtensions
    {
        public static object? Execute(this IMethod method, Dictionary<string, object> objects)
        {
            return method.MethodInfo.Invoke(method.Instance, [.. objects.OrderBy(kv => method.Parameters.Select(a => a.Name).ToList().IndexOf(kv.Key)).Select(a => a.Value)]);
        }

        public static object? ExecuteWithObjects(this IMethod method, object[] objects)
        {
            return method.MethodInfo.Invoke(method.Instance, objects);
        }
    }
}