using Qurl.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Qurl
{
    internal static class QueryParamsTokenizer
    {
        const string FilterCollectionSplit = @";(?=(?:[^']*'[^']*')*[^']*$)";
        const string FilterPattern = "^\\s*([\\w\\.\\-]+|'[\\w\\.\\s\\-]+')\\s*([^a-zA-Z0-9\\s\\;']+)\\s*(.*)$";
        const string CommaSeparatedValuesSplit = @",(?=(?:[^']*'[^']*')*[^']*$)";

        public static IEnumerable<(string PropName, string Operator, IEnumerable<string?> Values)> GetFilterTokens(string filterString)
        {
            var result = new List<(string PropName, string Operator, IEnumerable<string?> Values)>();

            if (string.IsNullOrEmpty(filterString))
                return result;

            var filtersCollection = Regex.Split(filterString, FilterCollectionSplit)
                .Where(s => !string.IsNullOrEmpty(s));

            foreach (var filter in filtersCollection)
            {
                var filterMatch = Regex.Match(filter, FilterPattern);

                if (filterMatch.Groups.Count != 4)
                    throw new QurlFormatException();

                result.Add((filterMatch.Groups[1].Value, filterMatch.Groups[2].Value, SplitCommaSeparatedValues(filterMatch.Groups[3].Value)));
            }

            return result;
        }

        public static IEnumerable<(string PropName, bool Ascending)> GetOrderingTokens(string orderByString)
        {
            var result = new List<(string PropName, bool Ascending)>();

            if (string.IsNullOrEmpty(orderByString))
                return result;

            var orderings = SplitCommaSeparatedValues(orderByString)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s!.Trim());

            foreach (var order in orderings)
            {
                if (order.StartsWith('-'))
                    result.Add((order[1..], false));
                else
                    result.Add((order, true));
            }

            return result;
        }

        private static IEnumerable<string?> SplitCommaSeparatedValues(string values)
        {
            return Regex.Split(values, CommaSeparatedValuesSplit)
                .Select(v => v.Trim())
                .Select(v => v == "null" ? null : v.TrimStart('\'').TrimEnd('\'').Replace("''", "'"));
        }
    }
}
