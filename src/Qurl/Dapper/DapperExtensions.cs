using System;
using System.Collections.Generic;
using System.Linq;

namespace Qurl.Dapper
{
    public static class DapperExtensions
    {
        private static string AddFilter(this string filters, string newFilter)
        {
            if (string.IsNullOrEmpty(filters))
                return newFilter;
            return filters + " AND " + newFilter;
        }

        public static QueryParts GetQueryParts<TFilter>(this Query<TFilter> query, string tableName, string tableAlias = "", string tableSchema = "", Dictionary<string, string> paramAliases = null)
            where TFilter : new()
        {
            if (paramAliases == null) paramAliases = new Dictionary<string, string>();
            var tableNameOrAlias = string.IsNullOrEmpty(tableAlias) ? tableName : tableAlias;

            var queryFilters = "";
            var parameters = new Dictionary<string, object>();

            var filterProperties = query.Filter.GetType().GetCachedProperties();
            foreach (var filterProp in filterProperties)
            {
                if (!typeof(IFilterProperty).IsAssignableFrom(filterProp.PropertyType))
                    continue;

                var customFilterAttr = (CustomFilterAttribute)Attribute.GetCustomAttribute(filterProp, typeof(CustomFilterAttribute));

                if (customFilterAttr != null && string.IsNullOrEmpty(customFilterAttr.MappedName))
                    continue;

                var filterProperty = (dynamic)filterProp.GetValue(query.Filter);
                if (filterProperty == null) continue;

                var propertyNameMapping = customFilterAttr != null
                    ? new QueryNameMapping(filterProp.Name, customFilterAttr.MappedName, customFilterAttr.NullValueMappedName)
                    : query.GetPropertyMappedName(filterProp.Name);

                var calcTableAlias = paramAliases.ContainsKey(propertyNameMapping.GetName()) ? paramAliases[propertyNameMapping.GetName()] : tableAlias;

                if (TryGetSqlFilter(filterProperty, propertyNameMapping.GetName(), out (string queryFilter, Dictionary<string, object> parameters) result, calcTableAlias))
                {
                    queryFilters = AddFilter(queryFilters, result.queryFilter);
                    parameters = parameters.Concat(result.parameters).ToDictionary(p => p.Key, p => p.Value);
                }
            }

            var completeTableName = string.IsNullOrEmpty(tableSchema) ? $"[{tableName}]" : $"[{tableSchema}]." + $"[{tableName}]";
            var sortAndPaging = query.GetSortAndPaging();

            return new QueryParts
            {
                Fields = $"{completeTableName}.*",
                TableName = completeTableName,
                TableAlias = string.IsNullOrEmpty(tableAlias) ? "" : $"[{tableAlias}]",
                Filters = queryFilters,
                Parameters = parameters,
                Sort = sortAndPaging.sort,
                Paging = sortAndPaging.paging
            };
        }

        private static (string sort, string paging) GetSortAndPaging<TFilter>(this Query<TFilter> query)
            where TFilter : new()
        {
            var orderBy = "";
            foreach (var (property, direction) in query.Sorts)
            {
                if (!string.IsNullOrEmpty(orderBy)) orderBy += ", ";
                var sortProp = property.Replace(" ", "").Replace(";", "");
                orderBy += $"[{sortProp}] {(direction == SortDirection.Descending ? "DESC" : "")}";
            }
            if (string.IsNullOrEmpty(orderBy))
                return ("", "");

            var paging = "";
            if (query.Offset > 0 || query.Limit > 0)
                paging += $"OFFSET {query.Offset} ROWS";
            if (query.Limit > 0)
                paging += $" FETCH NEXT {query.Limit} ROWS ONLY";
            return (orderBy, paging);
        }

        private static bool TryGetSqlFilter<T>(
            this FilterProperty<T> filter, string columnName, out (string queryFilter, Dictionary<string, object> parameters) result, string tableAlias = "", string filterName = "")
        {
            result = ("", null);
            if (filter == null) return false;

            filterName = string.IsNullOrEmpty(filterName) ? columnName : filterName;
            var originalFilterName = string.IsNullOrEmpty(tableAlias) ? filterName : tableAlias + "_" + filterName;
            filterName = "@" + originalFilterName;

            columnName = string.IsNullOrEmpty(tableAlias) ? $"[{columnName}]" : $"[{tableAlias}].[{columnName}]";
            var queryResult = GetSqlFilter(filter as dynamic, columnName, filterName);
            if (string.IsNullOrEmpty(queryResult.Item1)) return false;
            result = (queryResult.Item1, queryResult.Item2);
            return true;
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this EqualsFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} = {filterName}", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this NotEqualsFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} <> {filterName}", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this LessThanFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} < {filterName}", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this LessThanOrEqualFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} <= {filterName}", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this GreaterThanFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} > {filterName}", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this GreaterThanOrEqualFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} >= {filterName}", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this ContainsFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} LIKE CONCAT('%', {filterName}, '%')", new Dictionary<string, object> { { filterName, filter.Value } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this InFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} IN {filterName}", new Dictionary<string, object> { { filterName, filter.Values } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this NotInFilterProperty<T> filter, string columnName, string filterName)
        {
            return ($"{columnName} NOT IN {filterName}", new Dictionary<string, object> { { filterName, filter.Values } });
        }

        private static (string queryFilter, Dictionary<string, object> parameters) GetSqlFilter<T>(this RangeFilterProperty<T> filter, string columnName, string filterName)
        {
            var queryFilter = "";
            var parameters = new Dictionary<string, object>();
            if (filter.From.IsSet)
            {
                queryFilter = queryFilter.AddFilter($"{columnName} >= {filterName}From");
                parameters.Add(filterName + "From", filter.From.Value);
            }
            if (filter.To.IsSet)
            {
                queryFilter = queryFilter.AddFilter($"{columnName} <= {filterName}To");
                parameters.Add(filterName + "To", filter.To.Value);
            }

            return (queryFilter, parameters);
        }
    }
}
