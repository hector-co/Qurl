using System.Collections.Generic;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public interface IFilter
    {
        Expression GetExpression(Expression property, IEnumerable<object?> values);
    }
}
