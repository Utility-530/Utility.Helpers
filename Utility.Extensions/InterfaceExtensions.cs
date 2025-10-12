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
        public static void SetData(this ISetData setParent, object parent) => setParent.Data = parent;

        public static object Value(this IGetValue getKey) => getKey.Value;
        public static void SetValue(this ISetValue setParent, object parent) => setParent.Value = parent;
        public static object IsSelected(this IGetIsSelected getKey) => getKey.IsSelected;
        public static void SetIsSelected(this ISetIsSelected setParent, bool parent) => setParent.IsSelected = parent;
    }
}
