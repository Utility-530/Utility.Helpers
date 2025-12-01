using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Utility.Helpers.Reflection
{
    public class PropertyInfoWrapper
    {
        public PropertyInfo _propertyInfo;
        private object _targetObject;

        public PropertyInfoWrapper(PropertyInfo propertyInfo, object targetObject)
        {
            _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            _targetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
        }

        // Method to retrieve the property value
        public object GetValue()
        {
            return _propertyInfo.GetValue(_targetObject);
        }
        public void SetValue(object value)
        {
            _propertyInfo.SetValue(_targetObject, value);
        }

        // Additional properties or methods can be added as needed
        public string Name => _propertyInfo.Name;
        public Type Type => _propertyInfo.PropertyType;
    }

    public class ObjectWrapper
    {
        private readonly object _wrappedObject;
        public readonly Dictionary<string, PropertyInfoWrapper> _propertyCache;

        public ObjectWrapper(object obj)
        {
            _wrappedObject = obj ?? throw new ArgumentNullException(nameof(obj));
            _propertyCache = new Dictionary<string, PropertyInfoWrapper>();

            // Cache the properties for fast access
            foreach (PropertyInfo property in _wrappedObject.GetType().GetProperties())
            {
                _propertyCache[property.Name] = new PropertyInfoWrapper(property, obj);
            }
        }

        public object Object => _wrappedObject;

        public object this[string propertyName]
        {
            get => Get(propertyName);
            set => Set(propertyName, value);
        }

        public object Get(string propertyName)
        {
            if (_propertyCache.TryGetValue(propertyName, out var propertyInfo))
            {
                return propertyInfo.GetValue();
            }

            throw new ArgumentException($"Property '{propertyName}' does not exist.");
        }

        public T? Get<T>(string propertyName)
        {
            object value = Get(propertyName);  // Call the non-generic Get() method

            if (value is T typedValue)
            {
                return typedValue;  // If the value can be cast to T, return it
            }
            if (value is null)
            {
                return default;  // If the value can be cast to T, return it
            }

            throw new InvalidCastException($"Cannot cast property '{propertyName}' to type '{typeof(T)}'.");
        }

        public void Set(string propertyName, object value)
        {
            if (_propertyCache.TryGetValue(propertyName, out var propertyInfo))
            {
                Type propertyType = propertyInfo.Type;
                Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                if (value == null || underlyingType.IsAssignableFrom(value.GetType()))
                {
                    propertyInfo.SetValue(value);
                }
                else
                {
                    throw new ArgumentException($"Invalid value type for property '{propertyName}'. Expected '{propertyInfo.Type}'.");
                }
            }
            else
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist.");
            }
        }
    }

    public static class ReflectionHelper
    {
        public static FieldInfo GetField(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? GetField(type.BaseType ?? throw new Exception("ubn 43"), name);
        }

        public static void SetFieldValue(this object instance, string name, object value)
        {
            GetField(instance.GetType(), name).SetValue(instance, value);
        }

        public static void SetFieldValue(this object instance, string name, object value, ref FieldInfo fieldInfo)
        {
            (fieldInfo ??= GetField(instance.GetType(), name)).SetValue(instance, value);
        }

        #region Obsolete

        [Obsolete]
        public static IEnumerable<(string, Func<object?>)> GetStaticMethods(this Type t, params object[] parameters)
        {
            return t
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(null, parameters))));
        }

        //[Obsolete]
        //public static IEnumerable<(string, MethodInfo)> StaticMethods(this Type t)
        //{
        //    return t
        //            .GetMethods(BindingFlags.Public | BindingFlags.Static)
        //                .Select(m => (m.GetDescription(), m));
        //}

        [Obsolete]
        public static IEnumerable<(string, Func<object?>)> GetMethods(this object instance, params object[] parameters)
        {
            return instance.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(instance, parameters))));
        }

        [Obsolete]
        public static string GetDescription(this MethodInfo methodInfo)
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

        #endregion Obsolete

        ///<summary>
        /// <a href="https://dotnetcoretutorials.com/2020/07/03/getting-assemblies-is-harder-than-you-think-in-c/"></a>
        /// </summary>
        public static IEnumerable<Assembly> GetAssemblies(Predicate<AssemblyName>? predicate = null)
        {
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();

            if (Assembly.GetEntryAssembly() is { } ass)
            {
                assembliesToCheck.Enqueue(ass);
            }

            while (assembliesToCheck.Any())
            {
                foreach (var (reference, assembly) in from reference in assembliesToCheck.Dequeue().GetReferencedAssemblies()
                                                      where (predicate?.Invoke(reference) ?? true) && loadedAssemblies.Contains(reference.FullName) == false
                                                      let assembly = Assembly.Load(reference)
                                                      select (reference, assembly))
                {
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    yield return assembly;
                }
            }
        }

        public static IEnumerable<Assembly> GetSolutionAssemblies()
        {
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                                .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
            return assemblies;
        }
    }
}