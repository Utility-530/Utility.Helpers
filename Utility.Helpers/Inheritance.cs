using System;
using System.Collections.Generic;

namespace Utility.Helpers
{
    public static class InheritanceHelper
    {
        public static IEnumerable<Type> Inheritance(this Type type, bool includeObject = true)
        {
            if (type == null)
            {
                throw new ArgumentException("Tklokuk");
            }
            if (type == typeof(object))
            {
                if (includeObject)
                {
                    yield return type;
                }

                yield break;
            }

            yield return type;

            foreach (Type itemType in type.BaseType.Inheritance())
            {
                yield return itemType;
            }
        }

        /// <summary>
        /// <a href="https://stackoverflow.com/questions/67394080/how-to-compute-the-level-of-inheritance-for-two-types"></a>
        /// </summary>
        public static int InheritanceLevel(this Type baseType, Type derivedType)
        {
            if (baseType == derivedType)
            {
                return 0;
            }

            if (derivedType == typeof(object) || derivedType == null)
            {
                throw new ArgumentException("The two types are not related by inheritance!");
            }
            return 1 + baseType.InheritanceLevel(derivedType.BaseType);
        }

        public static int LevelsOfInheritance(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentException("Tklokuk");
            }
            if (type == typeof(object))
            {
                return 0;
            }

            return 1 + type.BaseType.LevelsOfInheritance();
        }
    }
}