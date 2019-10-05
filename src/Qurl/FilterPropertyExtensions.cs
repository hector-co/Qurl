using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Qurl
{
    internal static class FilterPropertyExtensions
    {
        private const string SplitArrayValuesRegEx = @"((?:\s*"".*?""\s*)|[^,""]*)";

        internal static void SetValue<TValue>(this SingleValueFilterProperty<TValue> filterProperty, string value)
        {
            filterProperty.Value = ((JToken)value).ToObject<TValue>();
        }

        internal static void SetValue<TValue>(this InFilterProperty<TValue> filterProperty, string values)
        {
            filterProperty.Values = SplitValues<TValue>(values);
        }

        internal static void SetValue<TValue>(this NotInFilterProperty<TValue> filterProperty, string values)
        {
            filterProperty.Values = SplitValues<TValue>(values);
        }

        internal static void SetValue<TValue>(this RangeFilterProperty<TValue> filterProperty, string values)
        {
            if (string.IsNullOrEmpty(values))
                return;

            var fromToValues = SplitValues(values).ToArray();

            if (fromToValues.Length == 1)
                fromToValues = new[] { fromToValues[0], null };

            if (!string.IsNullOrEmpty(fromToValues[0]))
                filterProperty.From = new Seteable<TValue>(((JToken)fromToValues[0]).ToObject<TValue>());
            if (!string.IsNullOrEmpty(fromToValues[1]))
                filterProperty.To = new Seteable<TValue>(((JToken)fromToValues[1]).ToObject<TValue>());
        }

        private static List<TValue> SplitValues<TValue>(string values)
        {
            return SplitValues(values).Select(v => ((JToken)v).ToObject<TValue>()).ToList();
        }

        private static List<string> SplitValues(string values)
        {
            if (string.IsNullOrEmpty(values))
                return new List<string>();

            //TODO adjust regular expression...
            var matches = Regex.Matches(values, SplitArrayValuesRegEx);
            var result = new List<string>();
            var prevIsEmpy = false;
            foreach (Match match in matches)
            {
                if ((!string.IsNullOrEmpty(match.Value) || match.Index == 0) || string.IsNullOrEmpty(match.Value) && prevIsEmpy)
                    result.Add(match.Value);
                prevIsEmpy = string.IsNullOrEmpty(match.Value);
            }
            return result;
        }
    }
}

