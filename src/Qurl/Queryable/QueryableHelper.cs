using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Queryable
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

        public IQueryable<TModel> GetQueryable(IQueryable<TModel> source, bool applySort = true, bool selectFields = true)
        {
            var filterProperties = _query.Filter.GetType().GetCachedProperties();

            foreach (var filterProp in filterProperties)
            {
                if (!typeof(IFilterProperty).IsAssignableFrom(filterProp.PropertyType))
                    continue;

                var customFilterAttr = (CustomFilterAttribute)Attribute.GetCustomAttribute(filterProp, typeof(CustomFilterAttribute));
                if (customFilterAttr != null)
                    continue;

                var mapFilterAttr = (MapFilterAttribute)Attribute.GetCustomAttribute(filterProp, typeof(MapFilterAttribute));
                if (mapFilterAttr != null && string.IsNullOrEmpty(mapFilterAttr.MappedName))
                    continue;

                var filterProperty = (dynamic)filterProp.GetValue(_query.Filter);
                if (filterProperty == null) continue;

                var propertyNameMapping = _query.PropertyNameHasMapping(filterProp.Name) || mapFilterAttr == null
                    ? _query.GetPropertyMappedName(filterProp.Name)
                    : new QueryNameMapping(filterProp.Name, mapFilterAttr.MappedName, mapFilterAttr.NullValueMappedName);

                Expression<Func<TModel, bool>> predicate = GetPredicate(filterProperty, propertyNameMapping);
                source = source.Where(predicate);
            }

            if (applySort)
                source = ApplySortAndPaging(source);

            if (selectFields)
                source = ApplySelectFields(source);

            return source;
        }

        public IQueryable<TModel> ApplySortAndPaging(IQueryable<TModel> source)
        {
            var modelProperties = typeof(TModel).GetCachedProperties();
            var applyThenBy = false;

            foreach (var (property, direction) in _query.Sorts)
            {
                var sortPoperty = modelProperties.FirstOrDefault(p => p.Name.Equals(property, StringComparison.CurrentCultureIgnoreCase));
                if (sortPoperty == null) continue;
                var propName = _query.GetPropertyMappedName(sortPoperty.Name);
                Expression<Func<TModel, object>> predicate = GetSortExpression(propName.PropertyName);

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

            if (_query.Offset > 0)
                source = source.Skip(_query.Offset);
            if (_query.Limit > 0)
                source = source.Take(_query.Limit);

            return source;
        }

        public IQueryable<TModel> ApplySelectFields(IQueryable<TModel> source)
        {
            if (_query.Fields.Any())
                source = source.Select(GetProperties(_query.Fields));

            return source;
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(EqualsFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Equal(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(NotEqualsFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.NotEqual(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(LessThanFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.LessThan(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(LessThanOrEqualFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.LessThanOrEqual(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(GreaterThanFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.GreaterThan(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(GreaterThanOrEqualFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.GreaterThanOrEqual(property, Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(ContainsFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Call(property, typeof(TProperty).GetMethod("Contains", new[] { typeof(TProperty) }), Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(InFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Values == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Call(Expression.Constant(filter.Values), typeof(List<TProperty>).GetMethod("Contains"), property);
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(NotInFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Values == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Not(Expression.Call(Expression.Constant(filter.Values), typeof(List<TProperty>).GetMethod("Contains"), property));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(RangeFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName();
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

        private static Expression<Func<TModel, TModel>> GetProperties(IEnumerable<string> propertyNames)
        {
            var modelParameter = Expression.Parameter(typeof(TModel), "m");

            var bindings = propertyNames.Select(propName =>
            {
                var propInfo = typeof(TModel).GetCachedProperties().FirstOrDefault(pr => pr.Name.Equals(propName, StringComparison.CurrentCultureIgnoreCase));
                var sourceValue = Expression.Property(modelParameter, propInfo);

                return Expression.Bind(propInfo, sourceValue);
            }).ToList();

            var newInstance = Expression.New(typeof(TModel));
            var initializer = Expression.MemberInit(newInstance, bindings);

            return Expression.Lambda<Func<TModel, TModel>>(initializer, modelParameter);
        }
    }
}
