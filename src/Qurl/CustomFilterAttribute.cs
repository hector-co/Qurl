using System;

namespace Qurl
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
