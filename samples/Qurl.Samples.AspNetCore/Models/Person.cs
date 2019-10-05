using Qurl;
using System;

namespace Qurl.Samples.AspNetCore.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public Group Group { get; set; }
        public bool Active { get; set; }
    }

    public class PersonFilter
    {
        public FilterProperty<int> Id { get; set; }
        public FilterProperty<string> Name { get; set; }
        public RangeFilterProperty<DateTime> Birthday { get; set; }
        [CustomFilter(MappedName = "Group.Id")]
        public FilterProperty<int> GroupId { get; set; }
        public EqualsFilterProperty<bool> Active { get; set; }
    }
}
