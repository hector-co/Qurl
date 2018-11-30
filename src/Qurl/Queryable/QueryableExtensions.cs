using System.Linq;

namespace Qurl.Queryable
{
    public static class QueryableExtensions
    {
        public static IQueryable<TModel> ApplyQuery<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query, bool applySortAndPaging = true, bool selectFields = true)
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

            return queryableHelper.ApplySortAndPaging(source);
        }

        public static IQueryable<TModel> SelectFields<TModel, TFilter>(this IQueryable<TModel> source, Query<TFilter> query)
            where TModel : class
            where TFilter : new()
        {
            var queryableHelper = new QueryableHelper<TModel, TFilter>(query);

            return queryableHelper.ApplySelectFields(source);
        }
    }
}
