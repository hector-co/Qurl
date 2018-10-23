using Qurl.Abstractions;
using System;

namespace Sample.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public bool Active { get; set; }
    }

    public class PersonFilter
    {
        public FilterProperty<int> Id { get; set; }
        public FilterProperty<string> Name { get; set; }
        public FilterProperty<DateTime> Birthday { get; set; }
        public FilterProperty<bool> Active { get; set; }
    }
}
