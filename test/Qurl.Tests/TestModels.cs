using System;

namespace Qurl.Tests
{
    //public class SampleObject
    //{
    //    public SampleObject()
    //    {
    //    }

    //    public SampleObject(int prop1, string prop2, bool prop3, DateTime prop4)
    //    {
    //        Prop1 = prop1;
    //        Prop2 = prop2;
    //        Prop3 = prop3;
    //        Prop4 = prop4;
    //    }

    //    public int Prop1 { get; set; }
    //    public string Prop2 { get; set; }
    //    public bool Prop3 { get; set; }
    //    public DateTime Prop4 { get; set; }
    //    public TestEnum Prop5 { get; set; }
    //}

    //public class SampleObjectWithRelationship
    //{
    //    public SampleObject Prop1 { get; set; }
    //}

    //public class SampleObjectFilter
    //{
    //    public FilterProperty<int> Prop1 { get; set; }
    //    public FilterProperty<string> Prop2 { get; set; }
    //    public FilterProperty<bool> Prop3 { get; set; }
    //    public FilterProperty<DateTime> Prop4 { get; set; }
    //}

    //public class SampleObjectWithRelationshipFilter
    //{
    //    public FilterProperty<int?> Prop1 { get; set; }

    //    [CustomFilter, MapFilter(NullValueMappedName = "Prop1")]
    //    public FilterProperty<int?> Prop1_2 { get; set; }
    //}

    //public class SampleObjectQueryWithDefaultSort : Query<SampleObjectFilter>
    //{
    //    public SampleObjectQueryWithDefaultSort() : base(defaultSort: new SortValue("prop1", SortDirection.Descending))
    //    {

    //    }
    //}

    public class TestModel1
    {
        public int IntProperty1 { get; set; }
        public string StringProperty1 { get; set; } = string.Empty;
        public double DoubleProperty1 { get; set; }
        public DateTime DateTimeProperty1 { get; set; }
        public TestEnum EnumProperty1 { get; set; }
        public bool BoolProperty1 { get; set; }
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}
