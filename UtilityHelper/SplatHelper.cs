using System;
using Splat;

namespace Utility
{
    public class SplatHelper
    {   
        /// <summary>
        /// Throws an exception if the 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetServiceUnSafe<T>(Type type)
        {
            if (Locator.Current.GetService(type) is T thing)
            {
                return thing;
            }

            if (Locator.Current.GetService(type) is { })
            {
                throw new Exception(
                    $"Unable to get object associated {type.Name} derived from {typeof(T).Name}. " +
                    $"Check that the type, {type.Name}, derives from {typeof(T).Name}");
            }

            throw new Exception(
                $"Unable to get object associated {type.Name}. Check that the type, {type.Name}, has been registered by Splat.");
        }

        public static object GetServiceUnSafe<T>()
        {
            return GetServiceUnSafe(typeof(T));
        }

        public static object GetServiceUnSafe(Type type)
        {
            if (Locator.Current.GetService(type) is { } thing)
            {
                return thing;
            }

            throw new Exception(
                $"Unable to get object associated {type.Name}. Check that the type, {type.Name}, has been registered by Splat.");
        }
    }
}
