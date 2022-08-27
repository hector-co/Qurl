﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public class InFilter<TValue> : FilterPropertyBase<TValue>
    {
        private List<TValue> _values;

        public InFilter()
        {
            _values = new List<TValue>();
        }

        public InFilter(IEnumerable<TValue> values)
        {
            _values = values.ToList();
        }

        public override IEnumerable<TValue> Values => _values.AsReadOnly();

        public override void SetValues(params object?[] values)
        {
            _values.Clear();
            _values.AddRange(values.Select(v => (TValue)v));
        }

        protected override Expression GetExpression(Expression property)
        {
            return Expression.Call(Expression.Constant(_values), typeof(List<TValue>).GetMethod("Contains"), property);
        }
    }
}
