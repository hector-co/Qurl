using System;
using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class CiEqualsFilter : SingleFilterPropertyBase<string>
    {
        public CiEqualsFilter()
        {
        }

        public CiEqualsFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            Expression toLowerExp = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            return Expression.Equal(toLowerExp, Expression.Constant(Value.ToLower()));
        }
    }
}
