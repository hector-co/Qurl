using System.Linq.Expressions;

namespace Qurl.Filters
{

    public class EndsWithFilter : SingleFilterPropertyBase<string>
    {
        public EndsWithFilter()
        {
        }

        public EndsWithFilter(string value) : base(value)
        {
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.Call(property, typeof(string).GetMethod("EndsWith", new[] { typeof(string) }), Expression.Constant(Value));
        }
    }
}
