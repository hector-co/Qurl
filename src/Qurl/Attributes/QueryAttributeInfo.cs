using System.Reflection;

namespace Qurl.Attributes
{
    internal class QueryAttributeInfo
    {
        public PropertyInfo? PropertyInfo { get; set; }
        public bool IsIgnored { get; set; }
        public string ModelPropertyName { get; set; } = string.Empty;
        public bool CustomFiltering { get; set; }
        public bool IsSortable { get; set; }
    }
}
