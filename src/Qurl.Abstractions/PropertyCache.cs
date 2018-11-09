using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Qurl.Abstractions
{
    public static class PropertyCache
    {
        internal static ConcurrentDictionary<Type, PropertyInfo[]> Properties { get; set; } = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static PropertyInfo[] GetCachedProperties(this Type type)
        {
            if (Properties.ContainsKey(type))
            {
                if (Properties.TryGetValue(type, out var props))
                    return props;
            }
            var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            Properties.TryAdd(type, properties);
            return properties;
        }
    }
}