using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class CiContainsFilter : IFilter
    {
        public Expression GetExpression(Expression property, IEnumerable<object?> values)
        {
            Expression toLowerExp = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            var value = (string?)values.ElementAt(0);
            return Expression.Call(toLowerExp, typeof(string).GetMethod("Contains", new[] { typeof(string) }), Expression.Constant(value?.ToLower(), typeof(string)));
        }
    }
}
