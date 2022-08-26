using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Qurl.Exceptions;
using Qurl.Filters;

namespace Qurl
{
    public class FilterFactory
    {
        public const string EqualsFilterOp = "==";
        public const string NotEqualsFilterOp = "!=";
        public const string LessThanFilterOp = "<";
        public const string LessThanOrEqualsFilterOp = "<=";
        public const string GreaterThanFilterOp = ">";
        public const string GreaterThanOrEqualsFilterOp = ">=";
        public const string ContainsFilterOp = "_=_";
        public const string StartsWithFilterOp = "=_";
        public const string EndsWithFilterOp = "_=";
        public const string FromToFilterOp = "<->";
        public const string InFilterOp = "[]";
        public const string NotInFilterOp = "![]";

        public const string ValidOperatorPattern = "^[^a-zA-Z0-9\\s\\;']+$";

        private readonly Dictionary<string, Type> _filterTypes;

        public FilterFactory()
        {
            _filterTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            AddDefaultFilterTypes();
        }

        internal void AddFilterType(string @operator, Type filterType)
        {
            if (!Regex.IsMatch(@operator, ValidOperatorPattern))
                throw new QurlException("Invalid characters in operator");

            if (_filterTypes.ContainsKey(@operator))
                throw new QurlException($"Duplicated filter operator: '{@operator}'");
            _filterTypes.Add(@operator, filterType);
        }

        public IFilter Create<TValue>(string @operator, params string[] values)
        {
            return Create(@operator, typeof(TValue), values);
        }

        public IFilterProperty Create(string @operator, Type valueType, params string?[] values)
        {
            if (!_filterTypes.ContainsKey(@operator))
                throw new QurlFormatException($"Operator not found: '{@operator}'");

            if (@operator.Equals(ContainsFilterOp, StringComparison.InvariantCultureIgnoreCase) ||
                @operator.Equals(StartsWithFilterOp, StringComparison.InvariantCultureIgnoreCase) ||
                @operator.Equals(EndsWithFilterOp, StringComparison.InvariantCultureIgnoreCase))
            {
                if (valueType != typeof(string))
                    throw new QurlFormatException($"'{@operator}' only supports string type.");
            }

            if (@operator == EqualsFilterOp && values.Length > 1)
                @operator = InFilterOp;
            if (@operator == NotEqualsFilterOp && values.Length > 1)
                @operator = NotInFilterOp;

            var filterType = _filterTypes[@operator];
            var completeFilterType = filterType.IsGenericType
                ? filterType.MakeGenericType(valueType)
                : filterType;

            var filter = (IFilterProperty)completeFilterType.CreateInstance();
            filter.SetValueFromString(values);

            return filter;
        }

        private void AddDefaultFilterTypes()
        {
            AddFilterType(EqualsFilterOp, typeof(EqualsFilter<>));
            AddFilterType(NotEqualsFilterOp, typeof(NotEqualsFilter<>));
            AddFilterType(LessThanFilterOp, typeof(LessThanFilter<>));
            AddFilterType(LessThanOrEqualsFilterOp, typeof(LessThanOrEqualsFilter<>));
            AddFilterType(GreaterThanFilterOp, typeof(GreaterThanFilter<>));
            AddFilterType(GreaterThanOrEqualsFilterOp, typeof(GreaterThanOrEqualsFilter<>));
            AddFilterType(ContainsFilterOp, typeof(ContainsFilter));
            AddFilterType(StartsWithFilterOp, typeof(StartsWithFilter));
            AddFilterType(EndsWithFilterOp, typeof(EndsWithFilter));
            AddFilterType(FromToFilterOp, typeof(FromToFilter<>));
            AddFilterType(InFilterOp, typeof(InFilter<>));
            AddFilterType(NotInFilterOp, typeof(NotInFilter<>));
        }
    }
}
