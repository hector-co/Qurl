using System;

namespace Qurl.Abstractions
{
    public class CustomFilterAttribute : Attribute
    {
        public string PropertyPath { get; set; }

        public CustomFilterAttribute(string propertyPath = "")
        {
            PropertyPath = propertyPath;
        }
    }
}
