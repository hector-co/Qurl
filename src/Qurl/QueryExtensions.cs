using System.Linq;

namespace Qurl
{
    public static class QueryExtensions
    {
        public static IQueryable<TModel> ApplyQuery<TFilterModel, TModel>(this IQueryable<TModel> source, Query<TFilterModel, TModel> query, bool applyOrderingAndPaging = true)
            where TModel : class
        {
            source = query.ApplyTo(source, applyOrderingAndPaging);

            return source;
        }
    }
}
