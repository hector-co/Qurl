using System;
using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class CiStartsWithFilter : SingleFilterPropertyBase<string>
    {
        public CiStartsWithFilter()
        {
        }

        public CiStartsWithFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            Expression toLowerExp = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            return Expression.Call(toLowerExp, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), Expression.Constant(Value.ToLower()));
        }
    }
}
