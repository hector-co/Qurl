using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class LessThanOrEqualsFilter<TValue> : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            return Expression.LessThanOrEqual(property, Expression.Constant(values.ElementAt(0), typeof(TValue)));
        }
    }
}
