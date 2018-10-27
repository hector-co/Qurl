﻿using FluentAssertions;
using Qurl.Abstractions.Queryable;
using System;
using System.Linq;
using Xunit;

namespace Qurl.Abstractions.Tests
{
    public class QueryableExtensionsTests
    {
        private static SampleObject SampleoObject1 = new SampleObject(1, "stringVal1", true, new DateTime(2018, 1, 1));
        private static SampleObject SampleoObject2 = new SampleObject(2, "stringVal2", true, new DateTime(2018, 6, 6));
        private static SampleObject SampleoObject3 = new SampleObject(3, "newvalue1", false, new DateTime(2017, 3, 9));
        private static SampleObject SampleoObject4 = new SampleObject(4, "newvalue2", false, new DateTime(2017, 12, 11));
        private static SampleObject SampleoObject5 = new SampleObject(5, "custom", true, new DateTime(2016, 7, 7));

        private static SampleObject[] SampleOjectsCollection = new[]
        {
            SampleoObject1, SampleoObject2, SampleoObject3, SampleoObject4, SampleoObject5
        };

        [Fact]
        public void TestEqualsFilter1()
        {
            const int prop1FilterValue = 2;
            const int expectedCount = 1;
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new EqualsFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var result = SampleOjectsCollection.AsQueryable().ApplyQuery(query);
            result.Count().Should().Be(expectedCount);
            result.FirstOrDefault().Prop1.Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestEqualsFilter2()
        {
            const string prop2FilterValue = "newvalue1";
            const int expectedCount = 1;
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop2 = new EqualsFilterProperty<string>
            {
                Value = prop2FilterValue
            };

            var result = SampleOjectsCollection.AsQueryable().ApplyQuery(query);
            result.Count().Should().Be(expectedCount);
            result.FirstOrDefault().Prop2.Should().Be(prop2FilterValue);
        }

        [Fact]
        public void TestInFilter1()
        {
            var prop1FilterValues = new[] { 2, 4, 5, 8 };
            const int expectedCount = 3;
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new InFilterProperty<int>
            {
                Values = prop1FilterValues
            };

            var result = SampleOjectsCollection.AsQueryable().ApplyQuery(query);
            result.Count().Should().Be(expectedCount);
            result.Where(r => prop1FilterValues.Contains(r.Prop1)).Count().Should().Be(expectedCount);
        }

        [Fact]
        public void TestBetweenFilter1()
        {
            var dateTimeFrom = new DateTime(2017, 3, 1);
            var dateTimeTo = new DateTime(2018, 1, 1);
            const int expectedCount = 3;
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop4 = new BetweenFilterProperty<DateTime>
            {
                From = new Seteable<DateTime>(dateTimeFrom),
                To = new Seteable<DateTime>(dateTimeTo)
            };

            var result = SampleOjectsCollection.AsQueryable().ApplyQuery(query);
            result.Count().Should().Be(expectedCount);
            result.Where(r => r.Prop4 >= dateTimeFrom && r.Prop4 <= dateTimeTo).Count().Should().Be(expectedCount);
        }

        [Fact]
        public void TestContainsFilter1()
        {
            const string searchValue = "stringVal";
            const int expectedCount = 2;
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop2 = new ContainsFilterProperty<string>
            {
                Value = searchValue
            };

            var result = SampleOjectsCollection.AsQueryable().ApplyQuery(query);
            result.Count().Should().Be(expectedCount);
        }

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

        public class SampleObjectFilter
        {
            public FilterProperty<int> Prop1 { get; set; }
            public FilterProperty<string> Prop2 { get; set; }
            public FilterProperty<bool> Prop3 { get; set; }
            public FilterProperty<DateTime> Prop4 { get; set; }
        }
    }
}
