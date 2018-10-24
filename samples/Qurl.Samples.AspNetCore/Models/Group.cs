﻿using Qurl.Abstractions;

namespace Qurl.Samples.AspNetCore.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
    }

    public class GroupFilter
    {
        public FilterProperty<int> Id { get; set; }
        public FilterProperty<string> Title { get; set; }
        public ContainsFilterProperty<string> Description { get; set; }
        public EqualsFilterProperty<bool> Active { get; set; }
    }

    public class GroupQuery : Query<GroupFilter>
    {
        public GroupQuery()
        {
            DefaultSort = ("Id", SortDirection.Ascending);
        }
    }
}