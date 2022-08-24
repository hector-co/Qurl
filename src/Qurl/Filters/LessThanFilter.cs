using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class LessThanFilter<TValue> : SingleFilterPropertyBase<TValue>
    {
        public LessThanFilter()
        {
        }

        public LessThanFilter(TValue value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.LessThan(property, Expression.Constant(Value));
        }
    }
}
