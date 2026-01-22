using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utility.Helpers.Reflection
{
    public static class ConstructorFactory
    {
        public static object Create<T>() => (T?)Create(typeof(T)) ?? throw new Exception("VDS22sdsd");

        public static object? Create(Type type)
        {
            // string special case
            if (type == typeof(string))
                return string.Empty;

            // value types (structs)
            if (type.IsValueType)
                return Activator.CreateInstance(type)!;

            // already has empty ctor? use it   
            if (type.GetConstructor(Type.EmptyTypes) is { } emptyCtor)
                return emptyCtor.Invoke(null)!;

            // pick the *best* constructor (longest, most resolvable)
            if (SelectBestConstructor(type) is { } bestCtor)
                return
                    bestCtor.Invoke(
                        bestCtor.GetParameters()
                        .Select(p => Create(p.ParameterType))
                        .ToArray())!;

            return null;
        }

        public static ConstructorInfo? SelectBestConstructor(Type t)
        {
            // We prefer ctors where all parameters are resolvable
            // and with the highest arity (more complete object)
            return t.GetConstructors()
                .Where(c => c.GetParameters().All(a => Create(a.ParameterType) != null))
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();
        }
    }
}
