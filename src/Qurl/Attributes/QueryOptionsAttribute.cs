using System;

namespace Qurl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryOptionsAttribute : Attribute
    {
        public string ParamsPropertyName { get; set; } = string.Empty;
        public string ModelPropertyName { get; set; } = string.Empty;
        public bool IsSortable { get; set; } = true;
        public bool CustomFiltering { get; set; }
    }
}
