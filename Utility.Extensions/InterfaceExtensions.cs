using Utility.Interfaces.Generic;
using Utility.Interfaces.NonGeneric;

namespace Utility.Extensions
{
    public static class InterfaceExtensions
    {
        public static string Key(this IGetKey getKey) => getKey.Key;
        public static void SetKey(this ISetKey getKey, string key) => getKey.Key = key;

        public static T Parent<T>(this IGetParent<T> getKey) => getKey.Parent;
        public static void SetParent<T>(this ISetParent<T> setParent, T parent) => setParent.Parent = parent;

        public static object Data(this IGetData getKey) => getKey.Data;
        public static void SetData<T>(this ISetData setParent, object parent) => setParent.Data = parent;


    }
}
