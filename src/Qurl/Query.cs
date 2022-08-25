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

        public bool IsSet<TValue>(Expression<Func<TFilterModel, TValue>> selector)
        {
            var propName = GetPropertyName(selector);

            return _filters.Any(f => f.PropertyName.Equals(propName, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool TryGetFilters<TValue>(Expression<Func<TFilterModel, TValue>> selector, out IEnumerable<FilterPropertyBase<TValue>> filters, FilterBehavior filterBehavior = FilterBehavior.Normal)
        {
            var propName = GetPropertyName(selector);

            filters = _filters
                .Where(f => f.PropertyName.Equals(propName, StringComparison.InvariantCultureIgnoreCase)
                    && (filterBehavior == FilterBehavior.Normal
                        ? !f.CustomFiltering
                        : filterBehavior != FilterBehavior.CustomFiltering || f.CustomFiltering))
                .Cast<FilterPropertyBase<TValue>>();

            return filters.Count() > 0;
        }

        private (PropertyInfo? propertyInfo, IEnumerable<QueryBaseAttribute> attributes) GetPropertyAttributesWithNameMapping(string propertyName)
        {
            foreach (var key in _filterModelAttrs.Keys)
            {
                if (_filterModelAttrs[key]
                    .Any(a => a is QueryOptionsAttribute attr && attr.ParamsPropertyName.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return (key, _filterModelAttrs[key]);
                }
            }

            PropertyInfo? propInfo = propertyName.GetPropertyInfo<TFilterModel>();

            if (propInfo == null)
                return (null, Array.Empty<QueryBaseAttribute>());

            return (propInfo, _filterModelAttrs[propInfo]);
        }

        internal void AddFilter(string propertyName, Func<Type, IFilterProperty> filterFactory)
        {
            var (property, attributes) = GetPropertyAttributesWithNameMapping(propertyName);

            if (property == null)
                return;

            if (attributes.Any(a => a is QueryIgnoreAttribute))
                return;

            var optionsAttribute = (QueryOptionsAttribute?)attributes.FirstOrDefault(a => a is QueryOptionsAttribute);

            var filter = filterFactory(property.PropertyType);
            filter.SetOptions(property.Name, optionsAttribute?.ModelPropertyName ?? string.Empty, optionsAttribute?.CustomFiltering ?? false);

            _filters.Add(filter);
        }

        public void AddFilter<TValue>(Expression<Func<TFilterModel, TValue>> selector, FilterPropertyBase<TValue> filter)
        {
            var propName = GetPropertyName(selector);

            AddFilter(propName, filter);
        }

        public void AddFilter<TValue>(string propertyName, FilterPropertyBase<TValue> filter)
        {
            var (property, attributes) = GetPropertyAttributesWithNameMapping(propertyName);

            if (property == null)
                return;

            if (attributes.Any(a => a is QueryIgnoreAttribute))
                return;

            var optionsAttribute = (QueryOptionsAttribute?)attributes.FirstOrDefault(a => a is QueryOptionsAttribute);

            filter.SetOptions(property.Name, optionsAttribute?.ModelPropertyName ?? string.Empty, optionsAttribute?.CustomFiltering ?? false);

            _filters.Add(filter);
        }

        public void AddSort<TValue>(Expression<Func<TFilterModel, TValue>> selector, bool ascending)
        {
            var propName = GetPropertyName(selector);

            AddSort(propName, ascending);
        }

        public void AddSort(string propertyName, bool ascending)
        {
            var (property, attributes) = GetPropertyAttributesWithNameMapping(propertyName);

            if (property == null)
                return;

            if (attributes.Any(a => a is QueryIgnoreAttribute))
                return;

            var optionsAttribute = (QueryOptionsAttribute?)attributes.FirstOrDefault(a => a is QueryOptionsAttribute);

            if (optionsAttribute != null && !optionsAttribute.IsSortable)
                return;

            var sortValue = new SortValue
            {
                PropertyName = property.Name,
                ModelPropertyName = optionsAttribute?.ModelPropertyName ?? string.Empty,
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
