using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class StartsWithFilter : SingleFilterPropertyBase<string>
    {
        public StartsWithFilter()
        {
        }

        public StartsWithFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.Call(property, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), Expression.Constant(Value));
        }
    }
}
