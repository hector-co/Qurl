using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Qurl
{
    public static class TypeExtensions
    {
        public static bool IsValidQueryType(this Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() != typeof(Query<>))
                    return false;
            }
            else
            {
                if (!typeof(Query<>).IsAssignableFrom(type) && !IsSubclassOfRawGeneric(typeof(Query<>), type))
                    return false;
            }
            return true;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        internal static ConcurrentDictionary<Type, PropertyInfo[]> Properties { get; set; } = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static PropertyInfo[] GetCachedProperties(this Type type)
        {
            if (Properties.ContainsKey(type))
            {
                if (Properties.TryGetValue(type, out var props))
                    return props;
            }
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Properties.TryAdd(type, properties);
            return properties;
        }
    }
}
