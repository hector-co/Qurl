using Qurl.Exceptions;
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

        public IQueryable<TModel> GetQueryable(IQueryable<TModel> source, bool applySort = false)
        {
            var filterProperties = _query.Filter.GetType().GetCachedProperties();

            foreach (var filterProp in filterProperties)
            {
                if (!typeof(IFilterProperty).IsAssignableFrom(filterProp.PropertyType))
                    continue;

                var customFilterAttr = (CustomFilterAttribute)Attribute.GetCustomAttribute(filterProp, typeof(CustomFilterAttribute));
                if (customFilterAttr != null)
                    continue;

                var filterProperty = (dynamic)filterProp.GetValue(_query.Filter);
                if (filterProperty == null) continue;

                var propertyNameMapping = _query.PropertyNameHasMapping(filterProp.Name)
                    ? _query.GetPropertyMappedName(filterProp.Name)
                    : new QueryNameMapping(filterProp.Name);

                Expression<Func<TModel, bool>> predicate = GetPredicate(filterProperty, propertyNameMapping);
                source = source.Where(predicate);
            }

            if (applySort)
                source = ApplySortAndPaging(source);

            return source;
        }

        public IQueryable<TModel> ApplySortAndPaging(IQueryable<TModel> source)
        {
            var applyThenBy = false;

            foreach (var sortValue in _query.GetEvalSorts())
            {
                var propName = sortValue.PropertyName;
                if (_query.PropertyNameHasMapping(sortValue.PropertyName))
                    propName = _query.GetPropertyMappedName(sortValue.PropertyName).MappedName;

                if (!TryGetSortExpression(propName, out var predicate))
                {
                    continue;
                }

                if (sortValue.SortDirection == SortDirection.Ascending)
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

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(StartsWithFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Call(property, typeof(TProperty).GetMethod("StartsWith", new[] { typeof(TProperty) }), Expression.Constant(filter.Value));
            return Expression.Lambda<Func<TModel, bool>>(comparison, modelParameter);
        }

        private static Expression<Func<TModel, bool>> GetPredicate<TProperty>(EndsWithFilterProperty<TProperty> filter, QueryNameMapping nameMapping)
        {
            var propName = nameMapping.GetName(filter.Value == null);
            var (modelParameter, property) = GetModelParamaterAndProperty(propName);
            var comparison = Expression.Call(property, typeof(TProperty).GetMethod("EndsWith", new[] { typeof(TProperty) }), Expression.Constant(filter.Value));
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

        private static bool TryGetSortExpression(string propName, out Expression<Func<TModel, object>> sortExpression)
        {
            if (TryGetModelParamaterAndProperty(propName, out var result))
            {
                sortExpression = Expression.Lambda<Func<TModel, object>>(Expression.Convert(result.property, typeof(object)), result.modelParameter);
                return true;
            }
            sortExpression = null;
            return false;
        }

        private static (ParameterExpression modelParameter, Expression property) GetModelParamaterAndProperty(string propName)
        {
            var modelParameter = Expression.Parameter(typeof(TModel), "m");
            Expression property = modelParameter;

            foreach (var member in propName.Split('.'))
            {
                if (!property.Type.GetCachedProperties().Any(t => t.Name.Equals(member, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                property = Expression.Property(property, member);
            }

            return (modelParameter, property);
        }

        private static bool TryGetModelParamaterAndProperty(string propName, out (ParameterExpression modelParameter, Expression property) result)
        {
            var processed = false;
            var modelParameter = Expression.Parameter(typeof(TModel), "m");
            Expression property = modelParameter;

            foreach (var member in propName.Split('.'))
            {
                if (!property.Type.GetCachedProperties().Any(t => t.Name.Equals(member, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                processed = true;
                property = Expression.Property(property, member);
            }

            result = (modelParameter, property);
            return processed;
        }

        public static Expression<Func<TModel, TModel>> BuildSelector(IEnumerable<string> members)
        {
            var parameter = Expression.Parameter(typeof(TModel), "e");
            var body = NewObject(typeof(TModel), parameter, members.Select(m => m.Split('.')));
            return Expression.Lambda<Func<TModel, TModel>>(body, parameter);
        }

        static Expression NewObject(Type targetType, Expression source, IEnumerable<string[]> memberPaths, int depth = 0)
        {
            var bindings = new List<MemberBinding>();
            var target = Expression.Constant(null, targetType);
            foreach (var memberGroup in memberPaths.GroupBy(path => path[depth]))
            {
                var memberName = memberGroup.Key;
                var targetMember = Expression.PropertyOrField(target, memberName);
                var sourceMember = Expression.PropertyOrField(source, memberName);
                var childMembers = memberGroup.Where(path => depth + 1 < path.Length);
                var targetValue = !childMembers.Any() ? sourceMember :
                    NewObject(targetMember.Type, sourceMember, childMembers, depth + 1);
                bindings.Add(Expression.Bind(targetMember.Member, targetValue));
            }
            return Expression.MemberInit(Expression.New(targetType), bindings);
        }
    }
}
