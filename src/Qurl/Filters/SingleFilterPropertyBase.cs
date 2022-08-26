using Qurl.Exceptions;
using System.Collections.Generic;

namespace Qurl.Filters
{
    public abstract class SingleFilterPropertyBase<TValue> : FilterPropertyBase<TValue>
    {
        public TValue Value { get; set; }

        public override IEnumerable<TValue> Values => new[] { Value };

#pragma warning disable CS8618
        public SingleFilterPropertyBase()
#pragma warning restore CS8618
        {
        }

        public SingleFilterPropertyBase(TValue value)
        {
            Value = value;
        }

        public override void SetValueFromString(params string?[] values)
        {
            if (values.Length != 1)
                throw new QurlFormatException($"One parameters expected");

            if (values[0] == null)
            {
                Value = default;
            }
            else
            {
                Value = values[0].ConvertTo<TValue>();
            }
        }
    }
}
