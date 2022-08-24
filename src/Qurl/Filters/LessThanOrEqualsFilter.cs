using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class LessThanOrEqualsFilter<TValue> : SingleFilterPropertyBase<TValue>
    {
        public LessThanOrEqualsFilter()
        {
        }

        public LessThanOrEqualsFilter(TValue value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.LessThanOrEqual(property, Expression.Constant(Value));
        }
    }
}
