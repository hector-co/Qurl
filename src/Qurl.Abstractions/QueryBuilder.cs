using Microsoft.AspNetCore.WebUtilities;
using Qurl.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Qurl.Abstractions
{
    public static class QueryBuilder
    {
        private const string PropNameFilterTypeRegEx = @"(?:filter\.)?\.?(.*)?\[(.*)?\]";
        private const string PropNameWithouyFilterTypeRegEx = @"(?:filter\.)?\.?(.*)";

        public static object FromQueryString(Type queryType, string queryString)
        {
            if (!queryType.IsValidQueryType())
            {
                throw new QurlException($"{queryType.Name} is not a valid type");
            }

            var query = (dynamic)Activator.CreateInstance(queryType);

            var queryDictionary = QueryHelpers.ParseQuery(queryString);
            var filterPropType = (Type)GetFilterType(query as dynamic);
            var properties = filterPropType.GetCachedProperties();

            foreach (var kv in queryDictionary)
            {
                if (kv.Key.ToLower() == "select")
                    query.Select = kv.Value.ToString().Split(',').ToList();
                if (kv.Key.ToLower() == "q")
                    query.QueryString = kv.Value;
                else if (kv.Key.ToLower() == "sort")
                    query.Sorts = GetSort(kv.Value);
                else if (kv.Key.ToLower() == "page")
                {
                    if (!int.TryParse(kv.Value, out var page))
                        throw new QurlParameterFormatException("page");
                    query.Page = page;
                }
                else if (kv.Key.ToLower() == "pagesize")
                {
                    if (!int.TryParse(kv.Value, out var pageSize))
                        throw new QurlParameterFormatException("pageSize");
                    query.PageSize = pageSize;
                }
                else
                {
                    var (propertyName, @operator) = GetPropertyNameAndOperator(kv.Key);

                    var propInfo = properties
                        .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));

                    var genericType = propInfo != null
                        ? propInfo.PropertyType.GetGenericArguments().FirstOrDefault()
                        : typeof(string);

                    dynamic filter;
                    try
                    {
                        if (propInfo == null || propInfo.PropertyType.GetGenericTypeDefinition() == typeof(FilterProperty<>))
                            filter = GetFilterInstance(@operator, genericType, kv.Value);
                        else
                            filter = GetFilterInstance(propInfo.PropertyType, kv.Value);
                    }
                    catch (Exception)
                    {
                        throw new QurlParameterFormatException(propertyName);
                    }

                    if (propInfo != null)
                        propInfo.SetValue(query.Filter, filter);
                    else
                        query.SetExtraFilterValue(propertyName, filter);
                }
            }

            return query;
        }

        private static Type GetFilterType<TFilter>(Query<TFilter> query)
            where TFilter : new()
        {
            return typeof(TFilter);
        }

        private static List<(string property, SortDirection direction)> GetSort(string sortExpression)
        {
            var result = new List<(string property, SortDirection direction)>();

            var sorts = sortExpression.Split(',');
            sorts = sorts.Select(s => s.Trim()).ToArray();

            foreach (var sort in sorts)
            {
                if (string.IsNullOrEmpty(sort)) continue;
                if (sort.StartsWith("+"))
                    result.Add((sort.Substring(1), SortDirection.Ascending));
                if (sort.StartsWith("-"))
                    result.Add((sort.Substring(1), SortDirection.Descending));
                else
                    result.Add((sort, SortDirection.Ascending));
            }

            return result;
        }

        private static (string propertyName, string @operator) GetPropertyNameAndOperator(string queryKey)
        {
            var matches = Regex.Match(queryKey, PropNameFilterTypeRegEx);
            if (matches.Success)
                return (matches.Groups[1].Value, matches.Groups[2].Value.ToLower());

            matches = Regex.Match(queryKey, PropNameWithouyFilterTypeRegEx);
            if (matches.Success)
                return (matches.Groups[1].Value, "eq");

            return (queryKey, "eq");
        }

        private static dynamic GetFilterInstance(string @operator, Type genericType, string value)
        {
            var filterGenericType = GetFilterGenericType(@operator);
            var filterType = filterGenericType.MakeGenericType(genericType);

            var filterInstance = (dynamic)Activator.CreateInstance(filterType);
            FilterPropertyExtensions.SetValue(filterInstance, value);

            return filterInstance;
        }

        private static dynamic GetFilterInstance(Type filterPropertyType, string value)
        {
            var filterInstance = (dynamic)Activator.CreateInstance(filterPropertyType);
            FilterPropertyExtensions.SetValue(filterInstance, value);

            return filterInstance;
        }

        private static Type GetFilterGenericType(string @operator)
        {
            switch (@operator.ToLower())
            {
                case "eq":
                    return typeof(EqualsFilterProperty<>);
                case "neq":
                    return typeof(NotEqualsFilterProperty<>);
                case "lt":
                    return typeof(LessThanFilterProperty<>);
                case "lte":
                    return typeof(LessThanOrEqualFilterProperty<>);
                case "gt":
                    return typeof(GreaterThanFilterProperty<>);
                case "gte":
                    return typeof(GreaterThanOrEqualFilterProperty<>);
                case "ct":
                    return typeof(ContainsFilterProperty<>);
                case "in":
                    return typeof(InFilterProperty<>);
                case "nin":
                    return typeof(NotInFilterProperty<>);
                case "bt":
                    return typeof(BetweenFilterProperty<>);
                default:
                    throw new QurlException(nameof(@operator));
            }
        }
    }
}
