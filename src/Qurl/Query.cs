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
        private readonly List<IFilterProperty> _filters;
        private readonly List<SortValue> _orderBy;

        public Query()
        {
            _filters = new List<IFilterProperty>();
            _orderBy = new List<SortValue>();
        }

        public IEnumerable<IFilter> Filters => _filters.AsReadOnly();
        public IEnumerable<SortValue> OrderBy => _orderBy.AsReadOnly();
        public int Offset { get; set; }
        public int Limit { get; set; }

        public bool TryGetFilters<TValue>(Expression<Func<TFilterModel, TValue>> selector, out IEnumerable<FilterPropertyBase<TValue>> filters)
        {
            var propName = selector.GetPropertyInfo().Name;

            filters = _filters
                .Where(f => f.PropertyName.Equals(propName, StringComparison.InvariantCultureIgnoreCase))
                .Cast<FilterPropertyBase<TValue>>();

            return filters.Count() > 0;
        }

        public void AddFilter<TValue>(Expression<Func<TFilterModel, TValue>> selector, FilterPropertyBase<TValue> filter)
        {
            var propertyInfo = selector.GetPropertyInfo();

            AddFilter(propertyInfo.GetPropertyAttrInfo<TFilterModel>(), filter);
        }

        internal void AddFilter(string propertyName, Func<Type, IFilterProperty> filterFactory)
        {
            var queryAttributeInfo = propertyName.GetPropertyAttrInfo<TFilterModel>();

            if (queryAttributeInfo == null)
                return;

            AddFilter(queryAttributeInfo, filterFactory(queryAttributeInfo.PropertyInfo!.PropertyType));
        }

        private void AddFilter(QueryAttributeInfo? queryAttributeInfo, IFilterProperty filter)
        {
            if (queryAttributeInfo == null)
                return;

            if (queryAttributeInfo.IsIgnored || queryAttributeInfo.PropertyInfo == null)
                return;

            filter.SetOptions(queryAttributeInfo.PropertyInfo.Name, queryAttributeInfo.ModelPropertyName, queryAttributeInfo.CustomFiltering);

            _filters.Add(filter);
        }

        public void AddSort<TValue>(Expression<Func<TFilterModel, TValue>> selector, bool ascending)
        {
            var propertyInfo = selector.GetPropertyInfo();

            AddSort(propertyInfo.GetPropertyAttrInfo<TFilterModel>(), ascending);
        }

        public void AddSort(string propertyName, bool ascending)
        {
            var queryAttributeInfo = propertyName.GetPropertyAttrInfo<TFilterModel>();

            AddSort(queryAttributeInfo, ascending);
        }

        private void AddSort(QueryAttributeInfo? queryAttributeInfo, bool ascending)
        {
            if (queryAttributeInfo == null)
                return;

            if (queryAttributeInfo.IsIgnored)
                return;

            if (!queryAttributeInfo.IsSortable)
                return;

            var sortValue = new SortValue
            {
                PropertyName = queryAttributeInfo.PropertyInfo!.Name,
                ModelPropertyName = queryAttributeInfo.ModelPropertyName,
                Ascending = ascending
            };

            _orderBy.Add(sortValue);
        }
    }
}
