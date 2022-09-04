using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Qurl.Exceptions;
using Qurl.Filters;

namespace Qurl
{
    public class FilterFactory
    {
        public const string EqualsFilterOp = "==";
        public const string CiEqualsFilterOp = "==*";
        public const string NotEqualsFilterOp = "!=";
        public const string CiNotEqualsFilterOp = "!=*";
        public const string LessThanFilterOp = "<";
        public const string LessThanOrEqualsFilterOp = "<=";
        public const string GreaterThanFilterOp = ">";
        public const string GreaterThanOrEqualsFilterOp = ">=";
        public const string ContainsFilterOp = "-=-";
        public const string CiContainsFilterOp = "-=-*";
        public const string StartsWithFilterOp = "=-";
        public const string CiStartsWithFilterOp = "=-*";
        public const string EndsWithFilterOp = "-=";
        public const string CiEndsWithFilterOp = "-=*";
        public const string FromToFilterOp = "<->";
        public const string InFilterOp = "|=";
        public const string NotInFilterOp = "!|=";

        public const string ValidOperatorPattern = "^[^a-zA-Z0-9\\s\\;']+$";

        private readonly IEnumerable<string> StringOperators = new[]
        {
            CiEqualsFilterOp, CiNotEqualsFilterOp, ContainsFilterOp, CiContainsFilterOp,
            StartsWithFilterOp, CiStartsWithFilterOp, EndsWithFilterOp, CiEndsWithFilterOp
        };

        private readonly Dictionary<string, Type> _filterTypes;

        public FilterFactory()
        {
            _filterTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            AddDefaultFilterTypes();
        }

        public IEnumerable<string> Operators => _filterTypes.Keys;

        internal void AddFilterType(string @operator, Type filterType)
        {
            if (!Regex.IsMatch(@operator, ValidOperatorPattern))
                throw new QurlException("Invalid characters in operator");

            if (_filterTypes.ContainsKey(@operator))
                throw new QurlException($"Duplicated filter operator: '{@operator}'");
            _filterTypes.Add(@operator, filterType);
        }

        public IFilter Create(string @operator, Type valueType)
        {
            if (!_filterTypes.ContainsKey(@operator))
                throw new QurlFormatException($"Operator not found: '{@operator}'");

            if (valueType != typeof(string) && StringOperators.Contains(@operator))
                throw new QurlFormatException($"'{@operator}' only supports string type.");

            var filterType = _filterTypes[@operator];
            var completeFilterType = filterType.IsGenericType
                ? filterType.MakeGenericType(valueType)
                : filterType;

            return (IFilter)completeFilterType.CreateInstance();
        }

        private void AddDefaultFilterTypes()
        {
            AddFilterType(EqualsFilterOp, typeof(EqualsFilter<>));
            AddFilterType(CiEqualsFilterOp, typeof(CiEqualsFilter));
            AddFilterType(NotEqualsFilterOp, typeof(NotEqualsFilter<>));
            AddFilterType(CiNotEqualsFilterOp, typeof(CiNotEqualsFilter));
            AddFilterType(LessThanFilterOp, typeof(LessThanFilter<>));
            AddFilterType(LessThanOrEqualsFilterOp, typeof(LessThanOrEqualsFilter<>));
            AddFilterType(GreaterThanFilterOp, typeof(GreaterThanFilter<>));
            AddFilterType(GreaterThanOrEqualsFilterOp, typeof(GreaterThanOrEqualsFilter<>));
            AddFilterType(ContainsFilterOp, typeof(ContainsFilter));
            AddFilterType(CiContainsFilterOp, typeof(CiContainsFilter));
            AddFilterType(StartsWithFilterOp, typeof(StartsWithFilter));
            AddFilterType(CiStartsWithFilterOp, typeof(CiStartsWithFilter));
            AddFilterType(EndsWithFilterOp, typeof(EndsWithFilter));
            AddFilterType(CiEndsWithFilterOp, typeof(CiEndsWithFilter));
            AddFilterType(FromToFilterOp, typeof(FromToFilter<>));
            AddFilterType(InFilterOp, typeof(InFilter<>));
            AddFilterType(NotInFilterOp, typeof(NotInFilter<>));
        }
    }
}