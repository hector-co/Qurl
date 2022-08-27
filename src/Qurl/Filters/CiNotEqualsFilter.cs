using System;
using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class CiNotEqualsFilter : SingleFilterPropertyBase<string>
    {
        public CiNotEqualsFilter()
        {
        }

        public CiNotEqualsFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            Expression toLowerExp = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            return Expression.NotEqual(toLowerExp, Expression.Constant(Value.ToLower()));
        }
    }
}
