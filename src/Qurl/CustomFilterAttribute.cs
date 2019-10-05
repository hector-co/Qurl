using System;

namespace Qurl
{
    public class CustomFilterAttribute : Attribute
    {
        public string MappedName { get; set; }
        public string NullValueMappedName { get; set; }
    }
}
