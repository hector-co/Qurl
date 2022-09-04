using Qurl.Exceptions;
using Qurl.Filters;
using System.Collections.Generic;

namespace Qurl
{
    public class FilterRegistry
    {
        private readonly Dictionary<string, IFilter> _filters;

        public FilterRegistry()
        {
            _filters = new Dictionary<string, IFilter>();

            RegisterFilters();
        }

        private void RegisterFilters()
        {
            AddFilter(new EqualsFilter());
        }

        private void AddFilter(IFilter filter)
        {
            _filters.Add(filter.Operator, filter);
        }

        public IFilter GetFilter(string @operator)
        {
            if (!_filters.ContainsKey(@operator))
                throw new QurlFormatException($"Operator not found: '{@operator}'");

            return _filters[@operator];
        }
    }
}
