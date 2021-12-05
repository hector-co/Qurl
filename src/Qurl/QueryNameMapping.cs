namespace Qurl
{
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
}
