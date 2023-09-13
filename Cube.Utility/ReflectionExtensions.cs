using System;
using System.ComponentModel;
using System.Reflection;

namespace Cube.Utility
{
    public static class ReflectionExtensions
    {
        public static object GetDefaultValueForProperty(this PropertyInfo property)
        {
            var defaultAttr = property.GetCustomAttribute(typeof(DefaultValueAttribute));
            if (defaultAttr != null)
                return (defaultAttr as DefaultValueAttribute).Value;

            var propertyType = property.PropertyType;
            return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
        }


        /// <summary>
        /// Retrieves the default value for a given Type
        /// </summary>
        /// <typeparam name="T">The Type for which to get the default value</typeparam>
        /// <returns>The default value for Type T</returns>
        /// <remarks>
        /// If a reference Type or a System.Void Type is supplied, this method always returns null.  If a value type 
        /// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an 
        /// exception.
        /// </remarks>
        /// <seealso cref="GetDefault(Type)"/>
        public static T GetDefault<T>()
        {
            return (T)GetDefault(typeof(T));
        }

        /// <summary>
        /// Retrieves the default value for a given Type
        /// </summary>
        /// <param name="type">The Type for which to get the default value</param>
        /// <returns>The default value for <paramref name="type"/></returns>
        /// <remarks>
        /// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type 
        /// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an 
        /// exception.
        /// </remarks>
        /// <seealso cref="GetDefault(Type)"/>
        public static object GetDefault(Type type)
        {
            // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;

            // If the supplied Type has generic parameters, its default value cannot be determined
            if (type.ContainsGenericParameters)
            {
                var msg = "{" + MethodInfo.GetCurrentMethod() + "} Error: The supplied value type <" + type +
                          "> contains generic parameters, so the default value cannot be retrieved";
                throw new ArgumentException(msg);
            }

            // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct), return a 
            //  default instance of the value type
            if (type.IsPrimitive || !type.IsNotPublic)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    var msg = "{" + MethodInfo.GetCurrentMethod() + "} Error: The Activator.CreateInstance method could not " +
                              "create a default instance of the supplied value type <" + type +
                              "> (Inner Exception message: \"" + e.Message + "\")";
                    throw new ArgumentException(msg, e);
                }
            }

            // Fail with exception
            var err = "{" + MethodInfo.GetCurrentMethod() + "} Error: The supplied value type <" + type +
                      "> is not a publicly-visible type, so the default value cannot be retrieved";
            throw new ArgumentException(err);
        }
    }
}