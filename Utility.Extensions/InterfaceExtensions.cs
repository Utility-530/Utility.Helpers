using Utility.Interfaces.NonGeneric;

namespace Utility.Extensions
{
    public static class InterfaceExtensions
    {
        public static string Key(this IGetKey getKey) => getKey.Key;
        public static void SetKey(this ISetKey getKey, string key) => getKey.Key = key;
    }
}
