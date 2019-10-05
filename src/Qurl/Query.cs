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


    public class Query<TFilter>
        where TFilter : new()
    {
        private readonly Dictionary<string, QueryNameMapping> _propsNameMappings;
        private List<(string property, SortDirection direction)> _sorts;

        public Query()
        {
            _propsNameMappings = new Dictionary<string, QueryNameMapping>();
            Filter = new TFilter();
            Fields = new List<string>();
            ExtraFilters = new Dictionary<string, (Type type, IFilterProperty filter)>(StringComparer.OrdinalIgnoreCase);
            _sorts = new List<(string property, SortDirection direction)>();
        }

        public TFilter Filter { get; set; }
        public List<string> Fields { get; set; }
        public Dictionary<string, (Type type, IFilterProperty filter)> ExtraFilters { get; set; }
        public string QueryString { get; set; }
        public (string property, SortDirection direction) DefaultSort { get; set; }
        public List<(string property, SortDirection direction)> Sorts
        {
            get
            {
                if (!_sorts.Any())
                    return new List<(string property, SortDirection direction)> { DefaultSort };
                return _sorts;
            }
            set
            {
                _sorts = value;
            }
        }
        public int Offset { get; set; }
        public int Limit { get; set; }

        internal Type GetFilterType(string name)
        {
            if (!ExtraFilters.ContainsKey(name))
                return typeof(string);
            return ExtraFilters[name].type;
        }

        public void AddExtraFilter<TType>(string name)
        {
            if (ExtraFilters.ContainsKey(name))
                return;
            ExtraFilters.Add(name, (typeof(TType), null));
        }

        public void SetPropertyNameMapping(string propertyName, string propertyMappedName, string propertyNullValueMappedName = "")
        {
            if (_propsNameMappings.ContainsKey(propertyName))
                _propsNameMappings[propertyName] = new QueryNameMapping(propertyName, propertyMappedName, propertyNullValueMappedName);
            else
                _propsNameMappings.Add(propertyName, new QueryNameMapping(propertyName, propertyMappedName, propertyNullValueMappedName));
        }

        public QueryNameMapping GetPropertyMappedName(string propertyName)
        {
            if (!_propsNameMappings.ContainsKey(propertyName)) return new QueryNameMapping(propertyName);
            return _propsNameMappings[propertyName];
        }

        internal void SetExtraFilterValue(string name, IFilterProperty filterProperty)
        {
            if (!ExtraFilters.ContainsKey(name))
                AddExtraFilter<string>(name);
            var (type, filter) = ExtraFilters[name];
            ExtraFilters[name] = (type, filterProperty);
        }
    }
}
