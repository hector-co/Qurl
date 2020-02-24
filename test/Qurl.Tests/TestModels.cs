using System;

namespace Qurl.Tests
{
    public class SampleObject
    {
        public SampleObject()
        {
        }

        public SampleObject(int prop1, string prop2, bool prop3, DateTime prop4)
        {
            Prop1 = prop1;
            Prop2 = prop2;
            Prop3 = prop3;
            Prop4 = prop4;
        }

        public int Prop1 { get; set; }
        public string Prop2 { get; set; }
        public bool Prop3 { get; set; }
        public DateTime Prop4 { get; set; }
    }

    public class SampleObjectWithRelationship
    {
        public SampleObject Prop1 { get; set; }
    }

    public class SampleObjectFilter
    {
        public FilterProperty<int> Prop1 { get; set; }
        public FilterProperty<string> Prop2 { get; set; }
        public FilterProperty<bool> Prop3 { get; set; }
        public FilterProperty<DateTime> Prop4 { get; set; }
    }

    public class SampleObjectWithRelationshipFilter
    {
        public FilterProperty<int?> Prop1 { get; set; }

        [CustomFilter, MapFilter(NullValueMappedName = "Prop1")]
        public FilterProperty<int?> Prop1_2 { get; set; }
    }

}
