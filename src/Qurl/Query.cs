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
            AddFilter(selector.GetPropertyName(), (_) => filter);
        }

        internal void AddFilter(string propertyName, Func<Type, IFilterProperty> filterFactory)
        {
            if (propertyName.TryGetPropertyQueryInfo<TFilterModel>(out var queryAttributeInfo))
            {
                AddFilter(queryAttributeInfo!, filterFactory(queryAttributeInfo!.PropertyInfo.PropertyType));
            }
        }

        private void AddFilter(QueryAttributeInfo queryAttributeInfo, IFilterProperty filter)
        {
            if (queryAttributeInfo.IsIgnored)
                return;

            filter.SetOptions(queryAttributeInfo.PropertyInfo.Name, queryAttributeInfo.ModelPropertyName, queryAttributeInfo.CustomFiltering);

            _filters.Add(filter);
        }

        public void AddSort<TValue>(Expression<Func<TFilterModel, TValue>> selector, bool ascending)
        {
            AddSort(selector.GetPropertyName(), ascending);
        }

        public void AddSort(string propertyName, bool ascending)
        {
            if (propertyName.TryGetPropertyQueryInfo<TFilterModel>(out var queryAttributeInfo))
            {
                AddSort(queryAttributeInfo!, ascending);
            }
        }

        private void AddSort(QueryAttributeInfo queryAttributeInfo, bool ascending)
        {
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
