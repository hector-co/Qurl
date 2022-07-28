using Microsoft.AspNetCore.WebUtilities;
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
        const string SplitArrayValuesRegEx = @"((?:\s*"".*?""\s*)|[^,""]*)";

        const string EqualsOperation = "EQ";
        const string NotEqualsOperation = "NEQ";
        const string LessThanOperation = "LT";
        const string LessThanOrEqualOperation = "LTE";
        const string GreaterThanOperation = "GT";
        const string GreaterThanOrEqualOperation = "GTE";
        const string ContainsOperation = "CT";
        const string StartsWithOperation = "SW";
        const string EndsWithOperation = "EW";
        const string IncludeOperation = "IN";
        const string NotIncludeOperation = "NIN";
        const string RangeOperation = "RNG";

        public const string OffsetQueryField = "OFFSET";
        public const string LimitQueryField = "LIMIT";
        public const string SortQueryField = "SORT";
        public const string FieldsQueryField = "FIELDS";

        const char ListSeparator = ',';
        const string SortAscending = "+";
        const string SortDescending = "-";

        public enum FieldType
        {
            FilterProperty,
            SortProperty,
            OffsetProperty,
            LimitProperty
        }

        public static object FromQueryString(Type queryType, string queryString, FilterMode mode = FilterMode.LHS)
        {
            if (!queryType.IsValidQueryType())
            {
                throw new QurlException($"{queryType.Name} is not a valid type");
            }

            var query = queryType.CreateInstance();

            var queryDictionary = QueryHelpers.ParseQuery(queryString);

            foreach (var kv in queryDictionary)
            {
                var values = string.Join(ListSeparator.ToString(), kv.Value);
                SetQueryValue(query, kv.Key, values, mode);
            }

            return query;
        }

        public static TQuery FromQueryString<TQuery>(string queryString, FilterMode mode = FilterMode.LHS)
        {
            return (TQuery)FromQueryString(typeof(TQuery), queryString, mode);
        }

        private static void SetQueryValue<TFilter>(Query<TFilter> query, string key, string values, FilterMode mode)
            where TFilter : new()
        {
            var fieldType = GetFieldType(key, mode);
            switch (fieldType)
            {
                case FieldType.FilterProperty:
                    SetPropertyFilterValue(query, key, values, mode);
                    break;
                case FieldType.SortProperty:
                    query.Sort = GetSort(values);
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

        private static FieldType GetFieldType(string key, FilterMode mode)
        {
            if (key.ToUpper() == OffsetQueryField)
                return FieldType.OffsetProperty;

            if (key.ToUpper() == LimitQueryField)
                return FieldType.LimitProperty;

            if (key.ToUpper() == SortQueryField)
                return FieldType.SortProperty;

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

        private static void SetPropertyFilterValue<TFilter>(Query<TFilter> query, string key, string values, FilterMode mode)
            where TFilter : new()
        {
            var properties = typeof(TFilter).GetCachedProperties();
            var splitedValues = SplitValues(values);
            var (propertyName, @operator, value) = GetPropertyNameOperatorAndValue(key, values, mode, splitedValues.Count > 1);

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
        }

        private static (string propertyName, string @operator, string value) GetPropertyNameOperatorAndValue(string queryKey, string values, FilterMode mode, bool valueIsArray)
        {
            if (mode == FilterMode.LHS)
            {
                var matches = Regex.Match(queryKey, PropNameFilterTypeRegEx);
                if (matches.Success)
                    return (matches.Groups[1].Value, matches.Groups[2].Value.ToLower(), values);

                matches = Regex.Match(queryKey, PropNameWithouyFilterTypeRegEx);
                if (matches.Success)
                    return (matches.Groups[1].Value, valueIsArray ? IncludeOperation : EqualsOperation, values);

                return (queryKey, valueIsArray ? IncludeOperation : EqualsOperation, values);
            }
            else
            {
                var matches = Regex.Match(queryKey, PropNameWithouyFilterTypeRegEx);
                if (matches.Success)
                    queryKey = matches.Groups[1].Value;

                var separatorIndex = values.IndexOf(':');
                
                if (separatorIndex < 0 && valueIsArray)
                    return (queryKey, IncludeOperation, values);

                if (separatorIndex < 0)
                    return (queryKey, EqualsOperation, values);

                var @operator = values.Substring(0, separatorIndex);
                var value = values.Substring(separatorIndex + 1);
                return (queryKey, @operator, value);
            }
        }

        private static dynamic GetFilterInstance(string @operator, Type genericType, string value)
        {
            var filterGenericType = GetFilterGenericType(@operator);
            var filterType = filterGenericType.MakeGenericType(genericType);

            var filterInstance = (IFilterProperty)filterType.CreateInstance();
            filterInstance.SetValueFromString(SplitValues(value).ToArray());

            return filterInstance;
        }

        private static dynamic GetFilterInstance(Type filterPropertyType, string value)
        {
            var filterInstance = (IFilterProperty)filterPropertyType.CreateInstance();
            filterInstance.SetValueFromString(SplitValues(value).ToArray());

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
                case StartsWithOperation:
                    return typeof(StartsWithFilterProperty<>);
                case EndsWithOperation:
                    return typeof(EndsWithFilterProperty<>);
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

        private static List<string> SplitValues(string values)
        {
            if (string.IsNullOrEmpty(values))
                return new List<string>();

            var matches = Regex.Matches(values, SplitArrayValuesRegEx);
            var result = new List<string>();
            var prevIsEmpy = false;
            foreach (Match match in matches)
            {
                if ((!string.IsNullOrEmpty(match.Value) || match.Index == 0) || string.IsNullOrEmpty(match.Value) && prevIsEmpy)
                    result.Add(match.Value);
                prevIsEmpy = string.IsNullOrEmpty(match.Value);
            }
            return result;
        }
    }
}

