using System;
using System.Collections.Generic;
using System.Linq;

namespace Qurl
{
    public enum SortDirection
    {
        Ascending, Descending
    }

    public struct QueryNameMapping
    {
        public QueryNameMapping(string propertyName, string mappedName = "", string nullValueNameFallback = "")
        {
            PropertyName = propertyName;
            MappedName = mappedName;
            NullValueMappedName = nullValueNameFallback;
        }

        public string PropertyName { get; set; }
        public string MappedName { get; set; }
        public string NullValueMappedName { get; set; }

        public string GetName(bool tryApplyNullFallback = false)
        {
            if (tryApplyNullFallback && !string.IsNullOrEmpty(NullValueMappedName))
                return NullValueMappedName;
            if (!string.IsNullOrEmpty(MappedName))
                return MappedName;
            return PropertyName;
        }
    }

    public class SortValue
    {
        public SortValue(string propertyName = default, SortDirection sortDirection = default)
        {
            PropertyName = propertyName;
            SortDirection = sortDirection;
        }

        public string PropertyName { get; set; }
        public SortDirection SortDirection { get; set; }
    }

    public class Query<TFilter>
        where TFilter : new()
    {
        private readonly Dictionary<string, QueryNameMapping> _propsNameMappings;
        private readonly Dictionary<string, (Type type, IFilterProperty filter)> _extraFilters;
        private readonly SortValue _defaultSort;

        public Query()
        {
            _propsNameMappings = new Dictionary<string, QueryNameMapping>();
            Filter = new TFilter();
            Fields = new List<string>();
            _extraFilters = new Dictionary<string, (Type type, IFilterProperty filter)>(StringComparer.OrdinalIgnoreCase);
            Sorts = new List<SortValue>();
        }

        public Query(SortValue defaultSort) : this()
        {
            _defaultSort = defaultSort;
        }

        public TFilter Filter { get; set; }
        public List<string> Fields { get; set; }
        //public List<(string property, SortDirection direction)> Sorts { get; set; }
        public List<SortValue> Sorts { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }

        internal Type GetFilterType(string name)
        {
            if (!_extraFilters.ContainsKey(name))
                return typeof(string);
            return _extraFilters[name].type;
        }

        public List<SortValue> GetEvalSorts()
        {
            if (Sorts != null && Sorts.Count > 0)
                return Sorts;

            if (!string.IsNullOrEmpty(_defaultSort.PropertyName))
                return new[] { _defaultSort }.ToList();

            return new List<SortValue>();
        }

        public void AddExtraFilter<TType>(string name)
        {
            if (_extraFilters.ContainsKey(name))
                return;
            _extraFilters.Add(name, (typeof(TType), null));
        }

        public void SetPropertyNameMapping(string propertyName, string propertyMappedName, string propertyNullValueMappedName = "")
        {
            if (_propsNameMappings.ContainsKey(propertyName))
                _propsNameMappings[propertyName] = new QueryNameMapping(propertyName, propertyMappedName, propertyNullValueMappedName);
            else
                _propsNameMappings.Add(propertyName, new QueryNameMapping(propertyName, propertyMappedName, propertyNullValueMappedName));
        }

        public IReadOnlyDictionary<string, (Type type, IFilterProperty filter)> GetExtraFilters()
        {
            return _extraFilters.ToDictionary(e => e.Key, e => e.Value);
        }

        public QueryNameMapping GetPropertyMappedName(string propertyName)
        {
            if (!_propsNameMappings.ContainsKey(propertyName)) return new QueryNameMapping(propertyName);
            return _propsNameMappings[propertyName];
        }

        public bool PropertyNameHasMapping(string propertyName)
        {
            return _propsNameMappings.ContainsKey(propertyName);
        }

        internal void SetExtraFilterValue(string name, IFilterProperty filterProperty)
        {
            if (!_extraFilters.ContainsKey(name))
                AddExtraFilter<string>(name);
            var (type, filter) = _extraFilters[name];
            _extraFilters[name] = (type, filterProperty);
        }
    }
}
