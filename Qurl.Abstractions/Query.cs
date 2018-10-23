using System;
using System.Collections.Generic;
using System.Linq;

namespace Qurl.Abstractions
{
    public enum SortDirection
    {
        Ascending, Descending
    }

    public class Query<TFilter>
        where TFilter : new()
    {
        private readonly Dictionary<string, string> _propsNameMappings;
        private List<(string property, SortDirection direction)> _sorts;

        public Query()
        {
            _propsNameMappings = new Dictionary<string, string>();
            Filter = new TFilter();
            Select = new List<string>();
            ExtraFilters = new Dictionary<string, (Type type, IFilterProperty filter)>(StringComparer.OrdinalIgnoreCase);
            _sorts = new List<(string property, SortDirection direction)>();
        }

        public TFilter Filter { get; set; }
        public List<string> Select { get; set; }
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
        public int Page { get; set; }
        public int PageSize { get; set; }

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

        public void SetPropertyNameMapping(string propertyName, string propertyMappedName)
        {
            if (_propsNameMappings.ContainsKey(propertyName)) return;
            _propsNameMappings.Add(propertyName, propertyMappedName);
        }

        public string GetPropertyMappedName(string propertyName)
        {
            if (!_propsNameMappings.ContainsKey(propertyName)) return propertyName;
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
