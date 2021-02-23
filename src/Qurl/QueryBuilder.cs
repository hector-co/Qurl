using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Qurl.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Qurl
{
    public enum FilterMode
    {
        LHS,
        RHS
    }

    public static class QueryBuilder
    {
        const string PropNameFilterTypeRegEx = @"(?i:filter\.)?\.?(.*)?\[(.*)?\]";
        const string PropNameWithouyFilterTypeRegEx = @"(?i:filter\.)?\.?(.*)";

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
        const string FieldsQueryField = "FIELDS";

        const char ListSeparator = ',';
        const string SortAscending = "+";
        const string SortDescending = "-";

        public enum FieldType
        {
            FilterProperty,
            SortProperty,
            OffsetProperty,
            LimitProperty,
            FieldsProperty
        }

        public static object FromQueryString(Type queryType, string queryString, FilterMode mode = FilterMode.LHS)
        {
            if (!queryType.IsValidQueryType())
            {
                throw new QurlException($"{queryType.Name} is not a valid type");
            }

            var query = (dynamic)Activator.CreateInstance(queryType);

            var queryDictionary = QueryHelpers.ParseQuery(queryString);

            foreach (var kv in queryDictionary)
            {
                SetQueryValue(query, kv.Key, kv.Value, mode);
            }

            return query;
        }

        public static TQuery FromQueryString<TQuery>(string queryString, FilterMode mode = FilterMode.LHS)
        {
            return (TQuery)FromQueryString(typeof(TQuery), queryString, mode);
        }

        private static void SetQueryValue<TFilter>(Query<TFilter> query, string key, StringValues values, FilterMode mode)
            where TFilter : new()
        {
            var fieldType = GetFieldType(key, mode);
            switch (fieldType)
            {
                case FieldType.FilterProperty:
                    SetPropertyFilterValue(query, key, values, mode);
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
                case FieldType.FieldsProperty:
                    query.Fields = values.ToString().Split(ListSeparator).ToList();
                    break;
            }
        }

        private static FieldType GetFieldType(string key, FilterMode mode)
        {
            if (key.ToUpper() == OffsetQueryField)
                return FieldType.OffsetProperty;

            if (key.ToUpper() == LimitQueryField)
                return FieldType.LimitProperty;

            if (key.ToUpper() == SortQueryField)
                return FieldType.SortProperty;

            if (key.ToUpper() == FieldsQueryField)
                return FieldType.FieldsProperty;

            if (mode == FilterMode.LHS && (Regex.Match(key, PropNameFilterTypeRegEx).Success || Regex.Match(key, PropNameWithouyFilterTypeRegEx).Success))
                return FieldType.FilterProperty;

            if (mode == FilterMode.RHS)
                return FieldType.FilterProperty;

            throw new QurlParameterFormatException();
        }

        private static List<SortValue> GetSort(string sortExpression)
        {
            var result = new List<SortValue>();

            var sorts = sortExpression.Split(ListSeparator);
            sorts = sorts.Select(s => s.Trim()).ToArray();

            foreach (var sort in sorts)
            {
                if (string.IsNullOrEmpty(sort)) continue;
                if (sort.StartsWith(SortAscending))
                    result.Add(new SortValue(sort.Substring(1), SortDirection.Ascending));
                if (sort.StartsWith(SortDescending))
                    result.Add(new SortValue(sort.Substring(1), SortDirection.Descending));
                else
                    result.Add(new SortValue(sort, SortDirection.Ascending));
            }

            return result;
        }

        private static void SetPropertyFilterValue<TFilter>(Query<TFilter> query, string key, StringValues values, FilterMode mode)
            where TFilter : new()
        {
            var properties = typeof(TFilter).GetCachedProperties();
            var (propertyName, @operator, value) = GetPropertyNameOperatorAndValue(key, values, mode);

            var propInfo = properties
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));

            var genericType = propInfo != null
                ? propInfo.PropertyType.GetGenericArguments().FirstOrDefault()
                : typeof(string);

            dynamic filter;
            try
            {
                if (propInfo == null || propInfo.PropertyType.GetGenericTypeDefinition() == typeof(FilterProperty<>))
                    filter = GetFilterInstance(@operator, genericType, value);
                else
                    filter = GetFilterInstance(propInfo.PropertyType, value);
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

        private static (string propertyName, string @operator, string value) GetPropertyNameOperatorAndValue(string queryKey, StringValues values, FilterMode mode)
        {
            var valueIsArray = values.Count > 1;

            if (mode == FilterMode.LHS)
            {
                var matches = Regex.Match(queryKey, PropNameFilterTypeRegEx);
                if (matches.Success)
                    return (matches.Groups[1].Value, matches.Groups[2].Value.ToLower(), values.ToString());

                matches = Regex.Match(queryKey, PropNameWithouyFilterTypeRegEx);
                if (matches.Success)
                    return (matches.Groups[1].Value, valueIsArray ? IncludeOperation : EqualsOperation, values.ToString());

                return (queryKey, valueIsArray ? IncludeOperation : EqualsOperation, values.ToString());
            }
            else
            {
                var matches = Regex.Match(queryKey, PropNameWithouyFilterTypeRegEx);
                if (matches.Success)
                    queryKey = matches.Groups[1].Value;

                if (valueIsArray)
                    return (queryKey, IncludeOperation, values.ToString());

                var separatorIndex = values[0].IndexOf(':');
                if (separatorIndex < 0)
                    return (queryKey, EqualsOperation, values.ToString());

                var @operator = values[0].Substring(0, separatorIndex);
                var value = values[0].Substring(separatorIndex + 1);
                return (queryKey, @operator, value);
            }
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

