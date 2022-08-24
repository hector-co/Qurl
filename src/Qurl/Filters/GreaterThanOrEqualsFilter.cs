using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class GreaterThanOrEqualsFilter<TValue> : SingleFilterPropertyBase<TValue>
    {
        public GreaterThanOrEqualsFilter()
        {
        }

        public GreaterThanOrEqualsFilter(TValue value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.GreaterThanOrEqual(property, Expression.Constant(Value));
        }
    }
}
