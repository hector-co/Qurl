﻿using Qurl.Exceptions;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qurl
{
    internal static class Extensions
    {
        internal static ConcurrentDictionary<Type, PropertyInfo[]> Properties { get; set; } = new ConcurrentDictionary<Type, PropertyInfo[]>();

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

        internal static object CreateInstance(this Type type)
        {
            NewExpression constructorExpression = Expression.New(type);
            Expression<Func<object>> lambdaExpression = Expression.Lambda<Func<object>>(constructorExpression);
            Func<object> createObjFunc = lambdaExpression.Compile();
            return createObjFunc();
        }

        internal static TValue ConvertTo<TValue>(this string? value)
        {
            if (value == null)
                return default;

            if (typeof(TValue).IsEnum)
            {
                if (!Enum.TryParse(typeof(TValue), value, true, out var enumValue))
                    throw new QurlFormatException($"'{value}' is not valid for type {typeof(TValue).Name}");

                return (TValue)enumValue;
            }
            else
            {
                if (!TypeDescriptor.GetConverter(typeof(TValue)).IsValid(value))
                    throw new QurlFormatException($"'{value}' is not valid for type {typeof(TValue).Name}");

                return (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFrom(value);
            }
        }
    }
}