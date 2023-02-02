using System;

namespace Utility.Helpers.Generic
{
    // from bphenriques/HelpersForNet

    /// <summary>
    /// Singleton pattern for a generic class that implements IDisposable and has a constructor with arity 0
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Singleton<T> where T : class, IDisposable, new()
    {
        private static object lockingObject = new object();
        private static T? singleTonObject;

        /// <summary>
        /// Returns the singleton instance
        /// </summary>
        public static T Instance => InstanceCreation();

        private static T InstanceCreation()
        {
            if (singleTonObject == null)
            {
                lock (lockingObject)
                {
                    if (singleTonObject == null)
                    {
                        singleTonObject = new T();
                    }
                }
            }
            return singleTonObject;
        }

        /// <summary>
        /// Disposes the singleton instance
        /// </summary>
        public static void Dispose()
        {
            Instance.Dispose();
            singleTonObject = null;
        }
    }

    /// <summary>
    /// Singleton pattern for a generic class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Singleton2<T> where T : class, new()
    {
        private static object lockingObject = new object();
        private static T? singleTonObject;

        /// <summary>
        /// Returns the singleton instance
        /// </summary>
        public static T Instance => InstanceCreation();

        private static T InstanceCreation()
        {
            if (singleTonObject == null)
            {
                lock (lockingObject)
                {
                    if (singleTonObject == null)
                    {
                        singleTonObject = new T();
                    }
                }
            }
            return singleTonObject;
        }
    }
}