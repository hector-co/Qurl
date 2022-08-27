using Qurl.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class FromToFilter<TValue> : FilterPropertyBase<TValue>
    {
        public TValue From { get; set; }
        public TValue To { get; set; }

        public override IEnumerable<TValue> Values => new[] { From, To };

#pragma warning disable CS8618
        public FromToFilter()
#pragma warning restore CS8618
        {

        }

        public FromToFilter(TValue from, TValue to)
        {
            From = from;
            To = to;
        }

        public override void SetValues(params object?[] values)
        {
            if (values.Length != 2)
                throw new QurlFormatException($"Two parameters expected");

            From = (TValue)values[0];
            To = (TValue)values[1];
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.And(
                Expression.GreaterThanOrEqual(property, Expression.Constant(From)),
                Expression.LessThanOrEqual(property, Expression.Constant(To)));
        }
    }
}
