using Qurl.Exceptions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Qurl
{
    public interface IFilterProperty
    {
        void SetValueFromString(params string[] values);
    }

    public abstract class FilterProperty<TValue> : IFilterProperty
    {
        public abstract void SetValueFromString(params string[] values);
    }

    public abstract class SingleValueFilterProperty<TValue> : FilterProperty<TValue>
    {
        public TValue Value { get; set; }

        public override void SetValueFromString(params string[] values)
        {
            if (values.Length != 1)
                throw new QurlParameterFormatException();
            Value = (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFrom(values[0]);
        }
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

    public class StartsWithFilterProperty<TValue> : SingleValueFilterProperty<TValue>
    {

    }

    public class EndsWithFilterProperty<TValue> : SingleValueFilterProperty<TValue>
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

        public override void SetValueFromString(params string[] values)
        {
            _values = new List<TValue>();
            foreach (var value in values)
                _values.Add((TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFrom(value));
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

        public override void SetValueFromString(params string[] values)
        {
            _values = new List<TValue>();
            foreach (var value in values)
                _values.Add((TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFrom(value));
        }
    }

    public class RangeFilterProperty<TValue> : FilterProperty<TValue>
    {
        public Seteable<TValue> From { get; set; }
        public Seteable<TValue> To { get; set; }

        public override void SetValueFromString(params string[] values)
        {
            var fromToValues = values;

            if (fromToValues.Length == 1)
                fromToValues = new[] { fromToValues[0], null };

            if (!string.IsNullOrEmpty(fromToValues[0]))
                From = new Seteable<TValue>((TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFrom(fromToValues[0]));
            if (!string.IsNullOrEmpty(fromToValues[1]))
                To = new Seteable<TValue>((TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFrom(fromToValues[1]));
        }
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
