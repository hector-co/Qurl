using System.Collections.Generic;
using System.Linq;

namespace Qurl
{
    public class FilterProperty<TValue> : IFilterProperty
    {
        private readonly List<TValue> _values;

        public FilterProperty(string name, string @operator)
        {
            _values = new List<TValue>();
            Name = name;
            Operator = @operator;
        }

        public string Name { get; internal set; } = string.Empty;

        public string Operator { get; internal set; } = string.Empty;

        public IEnumerable<TValue> Values => _values.AsReadOnly();

        public void SetValues(IEnumerable<object?> values)
        {
            _values.Clear();
            _values.AddRange(values.Select(v => (TValue)v));
        }
    }
}
