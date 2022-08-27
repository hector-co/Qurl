using Qurl.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qurl
{
    internal static class Extensions
    {
        internal static ConcurrentDictionary<Type, PropertyInfo[]> Properties { get; set; } = new ConcurrentDictionary<Type, PropertyInfo[]>();
        internal static ConcurrentDictionary<Type, Dictionary<PropertyInfo, IEnumerable<QueryBaseAttribute>>> TypesAttributes
            = new ConcurrentDictionary<Type, Dictionary<PropertyInfo, IEnumerable<QueryBaseAttribute>>>();

        internal static PropertyInfo[] GetCachedProperties(this Type type)
        {
            if (Properties.ContainsKey(type))
            {
                if (Properties.TryGetValue(type, out var props))
                    return props;
            }
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Properties.TryAdd(type, properties);
            return properties;
        }

        internal static QueryAttributeInfo? GetPropertyAttrInfo<TModel>(this string propertyName)
        {
            var type = typeof(TModel);
            if (!TypesAttributes.ContainsKey(type))
            {
                TypesAttributes.TryAdd(type, type
                    .GetCachedProperties()
                    .Select(p => (property: p, attributes:
                        Attribute.GetCustomAttributes(p, typeof(QueryBaseAttribute)).Select(a => (a as QueryBaseAttribute)!)))
                    .ToDictionary(f => f.property, f => f.attributes));
            }

            foreach (var key in TypesAttributes[type].Keys)
            {
                if (TypesAttributes[type][key]
                    .Any(a => a is QueryOptionsAttribute attr && attr.ParamsPropertyName.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return key.GetPropertyAttrInfo<TModel>();
                }
            }

            var propertyInfo = propertyName.GetPropertyInfo<TModel>();

            if (propertyInfo == null)
                return null;

            return propertyInfo.GetPropertyAttrInfo<TModel>();
        }

        internal static QueryAttributeInfo? GetPropertyAttrInfo<TModel>(this PropertyInfo propertyInfo)
        {
            var type = typeof(TModel);
            if (!TypesAttributes.ContainsKey(type))
            {
                TypesAttributes.TryAdd(type, type
                    .GetCachedProperties()
                    .Select(p => (property: p, attributes:
                        Attribute.GetCustomAttributes(p, typeof(QueryBaseAttribute)).Select(a => (a as QueryBaseAttribute)!)))
                    .ToDictionary(f => f.property, f => f.attributes));
            }

            if (!TypesAttributes[type].ContainsKey(propertyInfo))
                return null;

            var isIgnored = TypesAttributes[type][propertyInfo].Any(a => a is QueryIgnoreAttribute);

            if (isIgnored)
                return new QueryAttributeInfo
                {
                    PropertyInfo = propertyInfo,
                    IsIgnored = true
                };

            var optionsAttr = (QueryOptionsAttribute?)TypesAttributes[type][propertyInfo].FirstOrDefault(a => a is QueryOptionsAttribute);

            return new QueryAttributeInfo
            {
                PropertyInfo = propertyInfo,
                IsIgnored = false,
                ModelPropertyName = optionsAttr?.ModelPropertyName ?? string.Empty,
                CustomFiltering = optionsAttr?.CustomFiltering ?? false,
                IsSortable = optionsAttr?.IsSortable ?? true
            };
        }

        internal static Expression? GetPropertyExpression<TModel>(this string propertyName, Expression modelParameter)
        {
            Expression property = modelParameter;

            foreach (var member in propertyName.Split('.'))
            {
                var existentProp = property.Type.GetCachedProperties().FirstOrDefault(t => t.Name.Equals(member, StringComparison.InvariantCultureIgnoreCase));
                if (existentProp == null)
                    return null;

                property = Expression.Property(property, existentProp.Name);
            }

            return property;
        }

        internal static PropertyInfo? GetPropertyInfo<TModel>(this string propertyName)
        {
            return typeof(TModel).GetCachedProperties()
                .FirstOrDefault(t => t.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static PropertyInfo GetPropertyInfo<TModel, TValue>(this Expression<Func<TModel, TValue>> selector)
        {
            return (PropertyInfo)((MemberExpression)selector.Body).Member;
        }

        internal static object CreateInstance(this Type type)
        {
            NewExpression constructorExpression = Expression.New(type);
            Expression<Func<object>> lambdaExpression = Expression.Lambda<Func<object>>(constructorExpression);
            Func<object> createObjFunc = lambdaExpression.Compile();
            return createObjFunc();
        }

        internal static bool TryConvertTo(this string? value, Type targetType, out object? converted)
        {
            converted = null;

            if (value == null)
                return true;

            if (targetType.IsEnum)
            {
                if (!Enum.TryParse(targetType, value, true, out var enumValue))
                    return false;

                converted = enumValue;
                return true;
            }
            else
            {
                if (!TypeDescriptor.GetConverter(targetType).IsValid(value))
                    return false;

                converted = TypeDescriptor.GetConverter(targetType).ConvertFrom(value);
                return true;
            }
        }
    }
}
