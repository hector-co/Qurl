using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Qurl
{
    internal static class FilterPropertyExtensions
    {
        internal static void SetValue<TValue>(this SingleValueFilterProperty<TValue> filterProperty, string value)
        {
            filterProperty.Value = ((JToken)value).ToObject<TValue>();
        }

        internal static void SetValue<TValue>(this InFilterProperty<TValue> filterProperty, string values)
        {
            if (string.IsNullOrEmpty(values))
            {
                filterProperty.Values = new List<TValue>();
                return;
            }
            var result = new List<TValue>();
            var arrValues = values.Split(',');
            foreach (var arrValue in arrValues)
            {
                result.Add(((JToken)arrValue).ToObject<TValue>());
            }
            filterProperty.Values = result;
        }

        internal static void SetValue<TValue>(this NotInFilterProperty<TValue> filterProperty, string values)
        {
            if (string.IsNullOrEmpty(values))
            {
                filterProperty.Values = new List<TValue>();
                return;
            }
            var result = new List<TValue>();
            var arrValues = values.Split(',');
            foreach (var arrValue in arrValues)
            {
                result.Add(((JToken)arrValue).ToObject<TValue>());
            }
            filterProperty.Values = result;
        }

        internal static void SetValue<TValue>(this RangeFilterProperty<TValue> filterProperty, string values)
        {
            if (string.IsNullOrEmpty(values))
                return;

            var fromToValues = values.Split(',');

            if (fromToValues.Length == 1)
                fromToValues = new[] { fromToValues[0], null };

            if (!string.IsNullOrEmpty(fromToValues[0]))
                filterProperty.From = new Seteable<TValue>(((JToken)fromToValues[0]).ToObject<TValue>());
            if (!string.IsNullOrEmpty(fromToValues[1]))
                filterProperty.To = new Seteable<TValue>(((JToken)fromToValues[1]).ToObject<TValue>());
        }
    }
}
