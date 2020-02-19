using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Qurl.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Qurl
{
    public static class QueryBuilder
    {
        const string PropNameFilterTypeRegEx = @"(?:filter\.)?\.?(.*)?\[(.*)?\]";
        const string PropNameWithouyFilterTypeRegEx = @"(?:filter\.)?\.?(.*)";

        const string EqualsOperation = "EQ";
        const string NotEqualsOperation = "NEQ";
        const string LessThanOperation = "LT";
        const string LessThanOrEqualOperation = "LTE";
        const string GreaterThanOperation = "GT";
        const string GreaterThanOrEqualOperation = "GTE";
        const string ContainsOperation = "CT";
        const string IncludeOperation = "IN";
        const string NotIncludeOperation = "NIN";
        const string RangeOperation = "RNG";

        const string OffsetQueryField = "OFFSET";
        const string LimitQueryField = "LIMIT";
        const string SortQueryField = "SORT";

        const char SortSeparator = ',';
        const string SortAscending = "+";
        const string SortDescending = "-";

        public enum FieldType
        {
            FilterProperty,
            SortProperty,
            OffsetProperty,
            LimitProperty
        }

        public static object FromQueryString(Type queryType, string queryString)
        {
            if (!queryType.IsValidQueryType())
            {
                throw new QurlException($"{queryType.Name} is not a valid type");
            }

            var query = (dynamic)Activator.CreateInstance(queryType);

            var queryDictionary = QueryHelpers.ParseQuery(queryString);

            foreach (var kv in queryDictionary)
            {
                SetQueryValue(query, kv.Key, kv.Value);
            }

            return query;
        }

        private static void SetQueryValue<TFilter>(Query<TFilter> query, string key, StringValues values)
            where TFilter : new()
        {
            var fieldType = GetFieldType(key);
            switch (fieldType)
            {
                case FieldType.FilterProperty:
                    SetPropertyFilterValue(query, key, values);
                    break;
                case FieldType.SortProperty:
                    query.Sorts = GetSort(values);
                    break;
                case FieldType.OffsetProperty:
                    if (!int.TryParse(values, out var offset))
                        throw new QurlParameterFormatException(nameof(Query<TFilter>.Offset));
                    query.Offset = offset;
                    break;
                case FieldType.LimitProperty:
                    if (!int.TryParse(values, out var limit))
                        throw new QurlParameterFormatException(nameof(Query<TFilter>.Limit));
                    query.Limit = limit;
                    break;
            }
        }

        private static FieldType GetFieldType(string key)
        {
            if (key.ToUpper() == OffsetQueryField)
                return FieldType.OffsetProperty;

            if (key.ToUpper() == LimitQueryField)
                return FieldType.LimitProperty;

            if (key.ToUpper() == SortQueryField)
                return FieldType.SortProperty;

            if (Regex.Match(key, PropNameFilterTypeRegEx).Success || Regex.Match(key, PropNameWithouyFilterTypeRegEx).Success)
                return FieldType.FilterProperty;

            throw new QurlParameterFormatException();
        }

        private static List<(string property, SortDirection direction)> GetSort(string sortExpression)
        {
            var result = new List<(string property, SortDirection direction)>();

            var sorts = sortExpression.Split(SortSeparator);
            sorts = sorts.Select(s => s.Trim()).ToArray();

            foreach (var sort in sorts)
            {
                if (string.IsNullOrEmpty(sort)) continue;
                if (sort.StartsWith(SortAscending))
                    result.Add((sort.Substring(1), SortDirection.Ascending));
                if (sort.StartsWith(SortDescending))
                    result.Add((sort.Substring(1), SortDirection.Descending));
                else
                    result.Add((sort, SortDirection.Ascending));
            }

            return result;
        }

        private static void SetPropertyFilterValue<TFilter>(Query<TFilter> query, string key, StringValues values)
            where TFilter : new()
        {
            var properties = typeof(TFilter).GetCachedProperties();
            var (propertyName, @operator) = GetPropertyNameAndOperator(key, values.Count > 1);

            var propInfo = properties
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));

            var genericType = propInfo != null
                ? propInfo.PropertyType.GetGenericArguments().FirstOrDefault()
                : typeof(string);

            dynamic filter;
            try
            {
                if (propInfo == null || propInfo.PropertyType.GetGenericTypeDefinition() == typeof(FilterProperty<>))
                    filter = GetFilterInstance(@operator, genericType, values);
                else
                    filter = GetFilterInstance(propInfo.PropertyType, values);
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

        private static (string propertyName, string @operator) GetPropertyNameAndOperator(string queryKey, bool valueIsArray = false)
        {
            var matches = Regex.Match(queryKey, PropNameFilterTypeRegEx);
            if (matches.Success)
                return (matches.Groups[1].Value, matches.Groups[2].Value.ToLower());

            matches = Regex.Match(queryKey, PropNameWithouyFilterTypeRegEx);
            if (matches.Success)
                return (matches.Groups[1].Value, valueIsArray ? IncludeOperation : EqualsOperation);

            return (queryKey, valueIsArray ? IncludeOperation : EqualsOperation);
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
            switch (@operator.ToUpper())
            {
                case EqualsOperation:
                    return typeof(EqualsFilterProperty<>);
                case NotEqualsOperation:
                    return typeof(NotEqualsFilterProperty<>);
                case LessThanOperation:
                    return typeof(LessThanFilterProperty<>);
                case LessThanOrEqualOperation:
                    return typeof(LessThanOrEqualFilterProperty<>);
                case GreaterThanOperation:
                    return typeof(GreaterThanFilterProperty<>);
                case GreaterThanOrEqualOperation:
                    return typeof(GreaterThanOrEqualFilterProperty<>);
                case ContainsOperation:
                    return typeof(ContainsFilterProperty<>);
                case IncludeOperation:
                    return typeof(InFilterProperty<>);
                case NotIncludeOperation:
                    return typeof(NotInFilterProperty<>);
                case RangeOperation:
                    return typeof(RangeFilterProperty<>);
                default:
                    throw new QurlException(nameof(@operator));
            }
        }
    }
}

