using System;
using System.Collections.Generic;
using Qurl.Exceptions;
using Qurl.Filters;

namespace Qurl
{
    public class FilterFactory
    {
        public const string EqualsFilterOp = "EQ";
        public const string NotEqualsFilterOp = "NE";
        public const string LessThanFilterOp = "LT";
        public const string LessThanOrEqualsFilterOp = "LE";
        public const string GreaterThanFilterOp = "GT";
        public const string GreaterThanOrEqualsFilterOp = "GE";
        public const string ContainsFilterOp = "CT";
        public const string StartsWithFilterOp = "SW";
        public const string EndsWithFilterOp = "EW";
        public const string FromToFilterOp = "FT";
        public const string InFilterOp = "IN";
        public const string NotInFilterOp = "NI";

        private readonly Dictionary<string, Type> _filterTypes;

        public FilterFactory()
        {
            _filterTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            AddDefaultFilterTypes();
        }

        public void AddFilterType(string @operator, Type filterType)
        {
            _filterTypes.Add(@operator, filterType);
        }

        public IFilter Create<TValue>(string @operator, params string[] values)
        {
            return Create(@operator, typeof(TValue), values);
        }

        public IFilterProperty Create(string @operator, Type valueType, params string?[] values)
        {
            if (@operator.Equals(ContainsFilterOp, StringComparison.InvariantCultureIgnoreCase) ||
                @operator.Equals(StartsWithFilterOp, StringComparison.InvariantCultureIgnoreCase) ||
                @operator.Equals(EndsWithFilterOp, StringComparison.InvariantCultureIgnoreCase))
            {
                if (valueType != typeof(string))
                    throw new QurlException($"'{@operator}' only supports string type.");
            }

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
