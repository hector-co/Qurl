using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class EqualsFilter : IFilter
    {
        public string Operator => "==";

        public Expression GetExpression(Expression property, IEnumerable<object?> values, Type valueType)
        {
            return Expression.Equal(property, Expression.Constant(values.ElementAt(0), valueType));
        }
    }
}
