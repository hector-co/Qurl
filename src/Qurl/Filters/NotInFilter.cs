using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class NotInFilter<TValue> : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            return Expression.Not(Expression.Call(Expression.Constant(values.Cast<TValue>().ToList()), typeof(List<>).GetMethod("Contains"), property));
        }
    }
}
