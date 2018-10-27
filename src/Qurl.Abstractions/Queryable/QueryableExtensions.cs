using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Abstractions.Queryable
{
    internal class QueryableHelper<TModel, TFilter>
        where TModel : class
        where TFilter : new()
    {
        private readonly Query<TFilter> _query;

        public QueryableHelper(Query<TFilter> query)
        {
            _query = query;
        }

        public IQueryable<TModel> GetQueryable(IQueryable<TModel> source, bool applySort = true)
        {
            var filterProperties = _query.Filter.GetType().GetCachedProperties();

            foreach (var filterProp in filterProperties)
            {
                if (!typeof(IFilterProperty).IsAssignableFrom(filterProp.PropertyType))
                    continue;

                var customFilterAttr = (CustomFilterAttribute)Attribute.GetCustomAttribute(filterProp, typeof(CustomFilterAttribute));

                if (customFilterAttr != null && string.IsNullOrEmpty(customFilterAttr.PropertyPath))
                    continue;

                var filterProperty = (dynamic)filterProp.GetValue(_query.Filter);
                if (filterProperty == null) continue;
                var propName = customFilterAttr == null ? _query.GetPropertyMappedName(filterProp.Name) : customFilterAttr.PropertyPath;
                Expression<Func<TModel, bool>> predicate = GetPredicate(filterProperty, propName);
                source = source.Where(predicate);
            }

            if (applySort)
                source = ApplySort(source);

            return source;
        }

        public IQueryable<TModel> ApplySort(IQueryable<TModel> source)
        {
            var modelProperties = typeof(TModel).GetCachedProperties();
            var applyThenBy = false;

            foreach (var (property, direction) in _query.Sorts)
            {
                var sortPoperty = modelProperties.FirstOrDefault(p => p.Name.Equals(property, StringComparison.CurrentCultureIgnoreCase));
                if (sortPoperty == null) continue;
                var propName = _query.GetPropertyMappedName(sortPoperty.Name);
                Expression<Func<TModel, object>> predicate = GetSortExpression(propName);

                if (direction == SortDirection.Ascending)
                {
                    if (!applyThenBy)
                        source = source.OrderBy(predicate);
                    else
                        source = ((IOrderedQueryable<TModel>)source).ThenBy(predicate);
                }
                else
                {
                    if (!applyThenBy)
                        source = source.OrderByDescending(predicate);
                    else
                        source = ((IOrderedQueryable<TModel>)source).ThenByDescending(predicate);
                }

                applyThenBy = true;
            }

            if (_query.Page > 0)
                source = source.Skip((_query.Page - 1) * _query.PageSize);
            if (_query.PageSize > 0)
                source = source.Take(_query.PageSize);

            return source;
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(EqualsFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Equal(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(NotEqualsFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.NotEqual(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(LessThanFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.LessThan(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(LessThanOrEqualFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.LessThanOrEqual(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(GreaterThanFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.GreaterThan(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(GreaterThanOrEqualFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.GreaterThanOrEqual(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(ContainsFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Call(property, typeof(TProperty).GetMethod("Contains", new[] { typeof(TProperty) }), Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(InFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Call(Expression.Constant(filter.Values), typeof(List<TProperty>).GetMethod("Contains"), property);
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(NotInFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Not(Expression.Call(Expression.Constant(filter.Values), typeof(List<TProperty>).GetMethod("Contains"), property));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(BetweenFilterProperty<TProperty> filter, string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            if (!filter.From.IsSet && !filter.To.IsSet)
                return Expression.Lambda<Func<TModel, bool>>(Expression.Constant(true), modelParameter);
            BinaryExpression comparison;
            if (filter.From.IsSet && !filter.To.IsSet)
                comparison = Expression.GreaterThanOrEqual(property, Expression.Constant(filter.From.Value));
            else if (!filter.From.IsSet && filter.To.IsSet)
                comparison = Expression.LessThanOrEqual(property, Expression.Constant(filter.To.Value));
            else
                comparison = Expression.And(
                    Expression.GreaterThanOrEqual(property, Expression.Constant(filter.From.Value)),
                    Expression.LessThanOrEqual(property, Expression.Constant(filter.To.Value)));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, object>> GetSortExpression(string propName)
        {
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            Expression conversion = Expression.Convert(Expression.Property
                (modelParameter, propName), typeof(object));
            return Expression.Lambda<Func<TModel, object>>(conversion, modelParameter);
        }

        private static (ParameterExpression modelParameter, Expression property) GetModelParamaterAndProperty(string propName)
        {
            var modelParameter = Expression.Parameter(typeof(TModel), "m");
            Expression property = modelParameter;

            foreach (var member in propName.Split('.'))
            {
                property = Expression.Property(property, member);
            }

            return (modelParameter, property);
        }
    }

    public static class QueryableExtensions
    {
        public static IQueryable<TModel> ApplyQuery<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query, bool applySortAndPaging = true)
            where TModel : class
            where TFilter : new()
        {
            var queryableHelper = new QueryableHelper<TModel, TFilter>(query);

            return queryableHelper.GetQueryable(source, applySortAndPaging);
        }

        public static IQueryable<TModel> ApplySortAndPaging<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query)
            where TModel : class
            where TFilter : new()
        {
            var queryableHelper = new QueryableHelper<TModel, TFilter>(query);

            return queryableHelper.ApplySort(source);
        }
    }
}
