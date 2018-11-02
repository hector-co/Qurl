using System.Collections.Generic;
using System.Linq;

namespace Qurl.Abstractions
{
    public interface IFilterProperty
    {
    }

    public abstract class FilterProperty<TValue> : IFilterProperty
    {
    }

    public abstract class SingleValueFilterProperty<TValue> : FilterProperty<TValue>
    {
        public TValue Value { get; set; }
    }

    public class EqualsFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class NotEqualsFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class LessThanFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class LessThanOrEqualFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class GreaterThanFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class GreaterThanOrEqualFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class ContainsFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class InFilterProperty<TValue> : FilterProperty<TValue>
    {
        private List<TValue> _values;

        public IEnumerable<TValue> Values
        {
            get
            {
                return _values;
            }
            set
            {
                _values = value.ToList();
            }
        }
    }

    public class NotInFilterProperty<TValue> : FilterProperty<TValue>
    {
        private List<TValue> _values;

        public IEnumerable<TValue> Values
        {
            get
            {
                return _values;
            }
            set
            {
                _values = value.ToList();
            }
        }
    }

    public class RangeFilterProperty<TValue> : FilterProperty<TValue>
    {
        public Seteable<TValue> From { get; set; }
        public Seteable<TValue> To { get; set; }
    }

    public struct Seteable<TValue>
    {
        public TValue Value { get; private set; }
        public bool IsSet { get; private set; }

        public Seteable(TValue value)
        {
            Value = value;
            IsSet = true;
        }

    }

}
