using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class LessThanFilter<TValue> : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            return Expression.LessThan(property, Expression.Constant(values.ElementAt(0), typeof(TValue)));
        }
    }
}
