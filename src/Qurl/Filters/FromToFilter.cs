using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class FromToFilter<TValue> : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            return Expression.And(
                Expression.GreaterThanOrEqual(property, Expression.Constant(values.ElementAt(0), typeof(TValue))),
                Expression.LessThanOrEqual(property, Expression.Constant(values.ElementAt(1), typeof(TValue))));
        }
    }
}
