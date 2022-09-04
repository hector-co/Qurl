using Qurl.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl
{
    public class Query<TFilterModel> : Query<TFilterModel, TFilterModel>
    {

    }

    public class Query<TFilterModel, TModel>
    {
        private Expression<Func<TModel, bool>> _filterExp;
        private readonly List<(Expression<Func<TModel, object>> sortExp, bool ascending)> _orderBy;
        private readonly List<ICustomFilter> _customFilters;

        public Query()
        {
            _filterExp = (_) => true;
            _orderBy = new List<(Expression<Func<TModel, object>>, bool)>();
            _customFilters = new List<ICustomFilter>();
        }

        public int Offset { get; set; }
        public int Limit { get; set; }

        internal void SetFilterExpression(Expression<Func<TModel, bool>> filterExpression)
        {
            _filterExp = filterExpression;
        }

        internal void SetOrderBy(List<(Expression<Func<TModel, object>>, bool)> orderBy)
        {
            _orderBy.Clear();
            _orderBy.AddRange(orderBy);
        }

        internal void SetCustomFilters(List<ICustomFilter> cutomFilters)
        {
            _customFilters.Clear();
            _customFilters.AddRange(cutomFilters);
        }

        public bool TryGetCustomFilter<TValue>(Expression<Func<TFilterModel, TValue>> selector, out IEnumerable<CustomFilter<TValue>> filters)
        {
            var propName = selector.GetPropertyInfo().Name;

            filters = _customFilters
                .Where(f => f.Name.Equals(propName, StringComparison.InvariantCultureIgnoreCase))
                .Cast<CustomFilter<TValue>>();

            return filters.Count() > 0;
        }

        public IQueryable<TModel> ApplyTo(IQueryable<TModel> source, bool applyOrderingAndPaging = true)
        {
            source = source.Where(_filterExp);

            if (!applyOrderingAndPaging)
                return source;

            var applyThenBy = false;
            foreach (var (sortExp, ascending) in _orderBy)
            {
                source = ApplyOrder(source, sortExp, ascending, applyThenBy);
                applyThenBy = true;
            }

            if (Offset > 0)
                source = source.Skip(Offset);
            if (Limit > 0)
                source = source.Take(Limit);

            return source;
        }

        private static IQueryable<TModel> ApplyOrder(IQueryable<TModel> source, Expression<Func<TModel, object>> sortExp, bool ascendig, bool applyThenBy = true)
        {
            if (ascendig)
            {
                if (!applyThenBy)
                {
                    source = source.OrderBy(sortExp);
                }
                else
                {
                    source = ((IOrderedQueryable<TModel>)source).ThenBy(sortExp);
                }
            }
            else
            {
                if (!applyThenBy)
                {
                    source = source.OrderByDescending(sortExp);
                }
                else
                {
                    source = ((IOrderedQueryable<TModel>)source).ThenByDescending(sortExp);
                }
            }

            return source;
        }
    }
}
