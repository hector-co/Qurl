using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class ContainsFilter : SingleFilterPropertyBase<string>
    {
        public ContainsFilter()
        {
        }

        public ContainsFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.Call(property, typeof(string).GetMethod("Contains", new[] { typeof(string) }), Expression.Constant(Value));
        }
    }
}
