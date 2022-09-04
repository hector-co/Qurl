using Qurl.Exceptions;
using Qurl.Filters;
using Qurl.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Qurl
{
    internal class QueryableVisitor<TFilterModel, TModel> : IQueryVisitor
    {
        private readonly FilterFactory _filterFactory;
        private readonly QueryHelper _queryHelper;
        private readonly Stack<Expression?> _stack;
        private readonly List<ICustomFilter> _customFilters;
        private readonly ParameterExpression _modelParameter;

        public QueryableVisitor(FilterFactory filterFactory, QueryHelper queryHelper)
        {
            _filterFactory = filterFactory;
            _queryHelper = queryHelper;
            _stack = new Stack<Expression?>();
            _customFilters = new List<ICustomFilter>();
            _modelParameter = Expression.Parameter(typeof(TModel), "m");
        }

        public void Visit(OrElseNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

            var right = _stack.Pop();
            var left = _stack.Pop();

            if (left != null && right != null)
            {
                _stack.Push(Expression.OrElse(left, right));
                return;
            }

            var forPush = left == null && right == null
                ? null
                : left == null
                    ? right
                    : left;

            _stack.Push(forPush);
        }

        public void Visit(AndAlsoNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

            var right = _stack.Pop();
            var left = _stack.Pop();

            if (left != null && right != null)
            {
                _stack.Push(Expression.AndAlso(left, right));
                return;
            }

            var forPush = left == null && right == null
                ? null
                : left == null
                    ? right
                    : left;

            _stack.Push(forPush);
        }

        public void Visit(OperatorNode node)
        {
            if (!node.Property.TryGetPropertyQueryInfo<TFilterModel>(out var queryAttributeInfo) || queryAttributeInfo!.IsIgnored)
            {
                _stack.Push(null);
                return;
            }

            var valueType = queryAttributeInfo.PropertyInfo.PropertyType;
            var convertedValues = new List<object?>();
            foreach (var value in node.Values)
            {
                if (!value.TryConvertTo(valueType, out var converted))
                    throw new QurlFormatException($"'{value}' is not valid for type {valueType.Name}");

                if (valueType == typeof(DateTime))
                    converted = _queryHelper.ConvertDateTime((DateTime)converted!);

                if (valueType == typeof(DateTimeOffset))
                    converted = _queryHelper.ConvertDateTimeOffset((DateTimeOffset)converted!);

                convertedValues.Add(converted);
            }

            if (queryAttributeInfo.CustomFiltering)
            {
                var type = typeof(CustomFilter<>).MakeGenericType(valueType);
                var filter = (ICustomFilter)type.CreateInstance(queryAttributeInfo.PropertyInfo.Name, node.Operator);

                filter.SetValues(convertedValues);
                _customFilters.Add(filter);

                _stack.Push(null);
                return;
            }

            var propExp = queryAttributeInfo.ModelPropertyName.GetPropertyExpression<TModel>(_modelParameter);
            if (propExp == null)
            {
                _stack.Push(null);
                return;
            }

            _stack.Push(_filterFactory.Create(node.Operator, valueType).GetExpression(propExp, convertedValues));
        }

        public Expression<Func<TModel, bool>>? GetFilterExpression()
        {
            if (_stack.Count == 0 || _stack.TryPop(out var last) && last == null)
                return null;

            return Expression.Lambda<Func<TModel, bool>>(last, _modelParameter);
        }

        public List<ICustomFilter> GetCustomFilters() => _customFilters;
    }
}
