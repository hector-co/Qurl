using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Qurl.Filters
{
    public interface IFilter
    {
        string Operator { get; }
        Expression GetExpression(Expression property, IEnumerable<object?> values, Type valueType);
    }
}
