using System.Collections.Generic;

namespace Qurl.Filters
{
    public interface ICustomFilter
    {
        public string Operator { get; }
        public string Name { get; }
        void SetValues(IEnumerable<object?> values);
    }
}
