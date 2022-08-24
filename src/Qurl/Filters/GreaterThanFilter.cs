using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class GreaterThanFilter<TValue> : SingleFilterPropertyBase<TValue>
    {
        public GreaterThanFilter()
        {
        }

        public GreaterThanFilter(TValue value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.GreaterThan(property, Expression.Constant(Value));
        }
    }
}
