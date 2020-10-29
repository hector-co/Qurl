using System.Linq;

namespace Qurl.Queryable
{
    public static class QueryableExtensions
    {
        public static IQueryable<TModel> ApplyQuery<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query, bool applySortAndPaging = false, bool applySelectFields = false)
            where TModel : class
            where TFilter : new()
        {
            var queryableHelper = new QueryableHelper<TModel, TFilter>(query);

            return queryableHelper.GetQueryable(source, applySortAndPaging, applySelectFields);
        }

        public static IQueryable<TModel> ApplySortAndPaging<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query)
            where TModel : class
            where TFilter : new()
        {
            var queryableHelper = new QueryableHelper<TModel, TFilter>(query);

            return queryableHelper.ApplySortAndPaging(source);
        }

        public static IQueryable<TModel> ApplySelectFields<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query)
            where TModel : class
            where TFilter : new()
        {
            var queryableHelper = new QueryableHelper<TModel, TFilter>(query);

            return queryableHelper.ApplySelectFields(source);
        }
    }
}
