using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class StartsWithFilter : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            return Expression.Call(property, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), Expression.Constant(values.ElementAt(0), typeof(string)));
        }
    }
}
