using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class NotInFilter<TValue> : FilterPropertyBase<TValue>
    {
        private List<TValue> _values;

        public NotInFilter()
        {
            _values = new List<TValue>();
        }

        public NotInFilter(IEnumerable<TValue> values)
        {
            _values = values.ToList();
        }

        public IEnumerable<TValue> Values => _values.AsReadOnly();

        public override void SetValueFromString(params string?[] values)
        {
            _values.Clear();
            _values.AddRange(values.Select(v => v.ConvertTo<TValue>()));
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.Not(Expression.Call(Expression.Constant(_values), typeof(List<TValue>).GetMethod("Contains"), property));
        }
    }
}
