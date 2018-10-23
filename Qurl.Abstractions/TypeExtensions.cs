using System;

namespace Qurl.Abstractions
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
    }
}
