using Qurl.Exceptions;
using Qurl.Parser;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Qurl
{
    public class QueryBuilder
    {
        private readonly FilterFactory _filterFactory;
        private readonly QueryHelper _queryHelper;

        public QueryBuilder(FilterFactory filterFactory, QueryHelper queryHelper)
        {
            _filterFactory = filterFactory;
            _queryHelper = queryHelper;
        }

        public Query<TFilterModel, TFilterModel> CreateQuery<TFilterModel>(QueryModel queryModel)
        {
            return CreateQuery<TFilterModel, TFilterModel>(queryModel);
        }

        public Query<TFilterModel, TModel> CreateQuery<TFilterModel, TModel>(QueryModel queryModel)
        {
            var query = new Query<TFilterModel, TModel>
            {
                Offset = queryModel.Offset,
                Limit = queryModel.Limit
            };

            if (!string.IsNullOrEmpty(queryModel.Filter))
            {
                if (!QueryParser.TryParse(queryModel.Filter, out var root))
                    throw new QurlFormatException();

                var visitor = new QueryableVisitor<TFilterModel, TModel>(_filterFactory, _queryHelper);
                root!.Accept(visitor);

                var filterExp = visitor.GetFilterExpression();
                query.SetFilterExpression(filterExp);
                query.SetCustomFilters(visitor.GetCustomFilters());
            }

            query.SetOrderBy(GetOrderBy<TFilterModel, TModel>(queryModel.OrderBy));

            return query;
        }

        private static List<(Expression<Func<TModel, object>> sortExt, bool ascending)> GetOrderBy<TFilterModel, TModel>(string orderBy)
        {
            var result = new List<(Expression<Func<TModel, object>> sortExt, bool ascending)>();
            var orderingTokens = QueryParser.GetOrderingTokens(orderBy);
            foreach (var (propName, ascending) in orderingTokens)
            {
                if (!propName.TryGetPropertyQueryInfo<TFilterModel>(out var queryAttrInfo))
                    continue;

                if (queryAttrInfo!.IsIgnored || !queryAttrInfo!.IsSortable)
                    continue;

                var modelParameter = Expression.Parameter(typeof(TModel), "m");
                var propExp = queryAttrInfo.ModelPropertyName.GetPropertyExpression<TModel>(modelParameter);

                if (propExp == null)
                    continue;

                var sortExp = Expression.Lambda<Func<TModel, object>>(Expression.Convert(propExp, typeof(object)), modelParameter);

                result.Add((sortExp, ascending));
            }

            return result;
        }
    }
}
