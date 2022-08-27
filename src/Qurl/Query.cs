using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qurl.Filters;
using Qurl.Attributes;
using System.Linq.Expressions;
using Qurl.Exceptions;

namespace Qurl
{
    public class Query<TFilterModel>
    {
        private readonly Dictionary<PropertyInfo, IEnumerable<QueryBaseAttribute>> _filterModelAttrs;
        private readonly List<IFilterProperty> _filters;
        private readonly List<SortValue> _orderBy;

        public Query()
        {
            _filterModelAttrs = typeof(TFilterModel)
                .GetCachedProperties()
                .Select(p => (property: p, attributes:
                    Attribute.GetCustomAttributes(p, typeof(QueryBaseAttribute)).Select(a => (a as QueryBaseAttribute)!)))
                .ToDictionary(f => f.property, f => f.attributes);

            _filters = new List<IFilterProperty>();
            _orderBy = new List<SortValue>();
        }

        public IEnumerable<IFilter> Filters => _filters.AsReadOnly();
        public IEnumerable<SortValue> OrderBy => _orderBy.AsReadOnly();
        public int Offset { get; set; }
        public int Limit { get; set; }

        public bool TryGetFilters<TValue>(Expression<Func<TFilterModel, TValue>> selector, out IEnumerable<FilterPropertyBase<TValue>> filters)
        {
            var propName = GetPropertyName(selector);

            filters = _filters
                .Where(f => f.PropertyName.Equals(propName, StringComparison.InvariantCultureIgnoreCase))
                .Cast<FilterPropertyBase<TValue>>();

            return filters.Count() > 0;
        }

        private (PropertyInfo? propertyInfo, bool isIgnored, string modelPropertyName, bool customFiltering, bool isSortable) GetPropertyAndAttrValues(string propertyName)
        {
            foreach (var key in _filterModelAttrs.Keys)
            {
                if (_filterModelAttrs[key]
                    .Any(a => a is QueryOptionsAttribute attr && attr.ParamsPropertyName.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return GetPropertyAndAttrValues(key);
                }
            }

            PropertyInfo? propInfo = propertyName.GetPropertyInfo<TFilterModel>();

            if (propInfo == null)
                return (null, default, string.Empty, default, default);

            return GetPropertyAndAttrValues(propInfo);
        }

        private (PropertyInfo? propertyInfo, bool isIgnored, string modelPropertyName, bool customFiltering, bool isSortable) GetPropertyAndAttrValues(PropertyInfo propertyInfo)
        {
            var isIgnored = _filterModelAttrs[propertyInfo].Any(a => a is QueryIgnoreAttribute);

            var optionsAttr = (QueryOptionsAttribute?)_filterModelAttrs[propertyInfo].FirstOrDefault(a => a is QueryOptionsAttribute);

            return (propertyInfo, isIgnored, optionsAttr?.ModelPropertyName ?? string.Empty, optionsAttr?.CustomFiltering ?? false, optionsAttr?.IsSortable ?? true);
        }

        internal void AddFilter(string propertyName, Func<Type, IFilterProperty> filterFactory)
        {
            var (propertyInfo, isIgnored, modelPropertyName, customFiltering, _) = GetPropertyAndAttrValues(propertyName);

            if (propertyInfo == null)
                return;

            if (isIgnored)
                return;

            var filter = filterFactory(propertyInfo.PropertyType);
            filter.SetOptions(propertyInfo.Name, modelPropertyName, customFiltering);

            _filters.Add(filter);
        }

        public void AddFilter<TValue>(Expression<Func<TFilterModel, TValue>> selector, FilterPropertyBase<TValue> filter)
        {
            var propName = GetPropertyName(selector);

            AddFilter(propName, filter);
        }

        public void AddFilter<TValue>(string propertyName, FilterPropertyBase<TValue> filter)
        {
            var (propertyInfo, isIgnored, modelPropertyName, customFiltering, _) = GetPropertyAndAttrValues(propertyName);

            if (propertyInfo == null)
                return;

            if (isIgnored)
                return;

            filter.SetOptions(propertyInfo.Name, modelPropertyName, customFiltering);

            _filters.Add(filter);
        }

        public void AddSort<TValue>(Expression<Func<TFilterModel, TValue>> selector, bool ascending)
        {
            var propName = GetPropertyName(selector);

            AddSort(propName, ascending);
        }

        public void AddSort(string propertyName, bool ascending)
        {
            var (propertyInfo, isIgnored, modelPropertyName, _, isSortable) = GetPropertyAndAttrValues(propertyName);

            if (propertyInfo == null)
                return;

            if (isIgnored)
                return;

            if (!isSortable)
                return;

            var sortValue = new SortValue
            {
                PropertyName = propertyInfo.Name,
                ModelPropertyName = modelPropertyName,
                Ascending = ascending
            };

            _orderBy.Add(sortValue);
        }

        private static string GetPropertyName<TValue>(Expression<Func<TFilterModel, TValue>> selector)
        {
            if (!(selector.Body is MemberExpression member))
                throw new QurlException();

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new QurlException();

            return propInfo.Name;
        }
    }
}
