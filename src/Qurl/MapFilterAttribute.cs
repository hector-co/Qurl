using System;

namespace Qurl
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MapFilterAttribute : Attribute
    {
        public string MappedName { get; set; }
        public string NullValueMappedName { get; set; }
    }
}
