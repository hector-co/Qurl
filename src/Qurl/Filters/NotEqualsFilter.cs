using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class NotEqualsFilter<TValue> : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            return Expression.NotEqual(property, Expression.Constant(values.ElementAt(0), typeof(TValue)));
        }
    }
}
