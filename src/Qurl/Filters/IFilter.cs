using System.Linq.Expressions;

namespace Qurl.Filters
{
    public interface IFilter
    {
        Expression? GetFilterExpression<TModel>(ParameterExpression modelParameter);
    }
}
