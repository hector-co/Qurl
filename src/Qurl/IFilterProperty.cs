using System.Collections.Generic;

namespace Qurl
{
    public interface IFilterProperty
    {
        string Name { get; }
        string Operator { get; }
        void SetValues(IEnumerable<object?> values);
    }
}
