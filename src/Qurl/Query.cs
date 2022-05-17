using System;
using System.Collections.Generic;
using System.Linq;

namespace Qurl
{
    public enum SortDirection
    {
        Ascending, Descending
    }

    public class Query<TFilter>
        where TFilter : new()
    {
        private readonly Dictionary<string, QueryNameMapping> _propsNameMappings;
        private readonly Dictionary<string, (Type type, IFilterProperty filter)> _extraFilters;
        private readonly SortValue _defaultSort;

        public Query()
        {
            _propsNameMappings = new Dictionary<string, QueryNameMapping>(StringComparer.InvariantCultureIgnoreCase);
            Filter = new TFilter();
            Fields = new List<string>();
            _extraFilters = new Dictionary<string, (Type type, IFilterProperty filter)>(StringComparer.OrdinalIgnoreCase);
            Sort = new List<SortValue>();
            InitMappings();
        }

        public Query(SortValue defaultSort) : this()
        {
            _defaultSort = defaultSort;
        }

        public TFilter Filter { get; set; }
        public List<string> Fields { get; set; }
        public List<SortValue> Sort { get; set; }
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
            if (Sort != null && Sort.Count > 0)
                return Sort;

            if (!string.IsNullOrEmpty(_defaultSort?.PropertyName))
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

        private void InitMappings()
        {
            var filterProperties = Filter.GetType().GetCachedProperties();

            foreach (var filterProp in filterProperties)
            {
                var mapFilterAttr = (MapFilterAttribute)Attribute.GetCustomAttribute(filterProp, typeof(MapFilterAttribute));
                if (mapFilterAttr == null || string.IsNullOrEmpty(mapFilterAttr.MappedName))
                    continue;

                if (_propsNameMappings.ContainsKey(filterProp.Name))
                    continue;

                _propsNameMappings.Add(filterProp.Name, new QueryNameMapping(filterProp.Name, mapFilterAttr.MappedName, mapFilterAttr.NullValueMappedName));
            }
        }
    }
}
