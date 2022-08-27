using System;
using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class CiContainsFilter : SingleFilterPropertyBase<string>
    {
        public CiContainsFilter()
        {
        }

        public CiContainsFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            Expression toLowerExp = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            return Expression.Call(toLowerExp, typeof(string).GetMethod("Contains", new[] { typeof(string) }), Expression.Constant(Value.ToLower()));
        }
    }
}
