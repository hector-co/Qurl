using FluentAssertions;
using Newtonsoft.Json.Linq;
using Qurl.Exceptions;
using Qurl.Filters;
using System;
using System.Linq;
using Xunit;

namespace Qurl.Tests
{
    public class QueryBuilderTests
    {
        private readonly FilterFactory _filterFactory;
        private readonly QueryBuilder _queryBuilder;

        public QueryBuilderTests()
        {
            _filterFactory = new FilterFactory();
            _queryBuilder = new QueryBuilder(_filterFactory);

        }

        [Theory]
        [InlineData("intProperty1,-stringProperty1,doubleProperty1", new[] { "IntProperty1", "StringProperty1", "DoubleProperty1" }, new[] { true, false, true })]
        [InlineData("-enumProperty1,-dateTimeProperty1,-stringProperty1", new[] { "EnumProperty1", "DateTimeProperty1", "StringProperty1" }, new[] { false, false, false })]
        public void OrderByTest(string orderBy, string[] properties, bool[] ascending)
        {
            var queryParams = new QueryParams
            {
                OrderBy = orderBy
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.OrderBy.Count().Should().Be(properties.Length);
            for (var i = 0; i < query.OrderBy.Count(); i++)
            {
                query.OrderBy.ElementAt(i).PropertyName.Should().Be(properties[i]);
                query.OrderBy.ElementAt(i).Ascending.Should().Be(ascending[i]);
            }
        }

        [Theory]
        [InlineData("intPropertyx,-stringPropertyy,doublePropertyz")]
        [InlineData("-enumPropertyx,-dateTimePropertyy,-stringPropertyz")]
        public void OrderByShouldIgnoreNonExistentPropertiesTest(string orderBy)
        {
            var queryParams = new QueryParams
            {
                OrderBy = orderBy
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.OrderBy.Count().Should().Be(0);
        }

        [Fact]
        public void EqualsFilterTest()
        {
            var expectedIntValue = 8;
            var expectedBoolValue = false;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == {expectedIntValue}; boolProperty1 == {expectedBoolValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(EqualsFilter<int>));
            ((EqualsFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedIntValue);

            query.TryGetFilters(m => m.BoolProperty1, out var boolFilters).Should().BeTrue();

            boolFilters.Count().Should().Be(1);
            boolFilters.ElementAt(0).GetType().Should().Be(typeof(EqualsFilter<bool>));
            ((EqualsFilter<bool>)boolFilters.ElementAt(0)).Value.Should().Be(expectedBoolValue);
        }

        [Fact]
        public void EqualsFilterTestWithEnums()
        {
            var exptectedEnumValue = TestEnum.Value1;

            var queryParams = new QueryParams
            {
                Filter = $"enumProperty1 == {exptectedEnumValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.EnumProperty1, out var enumFilters).Should().BeTrue();

            enumFilters.Count().Should().Be(1);
            enumFilters.ElementAt(0).GetType().Should().Be(typeof(EqualsFilter<TestEnum>));
            ((EqualsFilter<TestEnum>)enumFilters.ElementAt(0)).Value.Should().Be(exptectedEnumValue);
        }

        [Fact]
        public void NotEqualsFilterTest()
        {
            var expectedIntValue = 8;
            var expectedBoolValue = false;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 != {expectedIntValue}; boolProperty1 != {expectedBoolValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(NotEqualsFilter<int>));
            ((NotEqualsFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedIntValue);

            query.TryGetFilters(m => m.BoolProperty1, out var boolFilters).Should().BeTrue();

            boolFilters.Count().Should().Be(1);
            boolFilters.ElementAt(0).GetType().Should().Be(typeof(NotEqualsFilter<bool>));
            ((NotEqualsFilter<bool>)boolFilters.ElementAt(0)).Value.Should().Be(expectedBoolValue);
        }

        [Fact]
        public void LessThanFilterTest()
        {
            var expectedIntValue = 8;
            var expectedBoolValue = false;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 < {expectedIntValue}; boolProperty1 < {expectedBoolValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(LessThanFilter<int>));
            ((LessThanFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedIntValue);

            query.TryGetFilters(m => m.BoolProperty1, out var boolFilters).Should().BeTrue();

            boolFilters.Count().Should().Be(1);
            boolFilters.ElementAt(0).GetType().Should().Be(typeof(LessThanFilter<bool>));
            ((LessThanFilter<bool>)boolFilters.ElementAt(0)).Value.Should().Be(expectedBoolValue);
        }

        [Fact]
        public void LessThanOrEqualFilterTest()
        {
            var expectedIntValue = 8;
            var expectedBoolValue = false;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 <= {expectedIntValue}; boolProperty1 <= {expectedBoolValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(LessThanOrEqualsFilter<int>));
            ((LessThanOrEqualsFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedIntValue);

            query.TryGetFilters(m => m.BoolProperty1, out var boolFilters).Should().BeTrue();

            boolFilters.Count().Should().Be(1);
            boolFilters.ElementAt(0).GetType().Should().Be(typeof(LessThanOrEqualsFilter<bool>));
            ((LessThanOrEqualsFilter<bool>)boolFilters.ElementAt(0)).Value.Should().Be(expectedBoolValue);
        }

        [Fact]
        public void GreaterThanFilterTest()
        {
            var expectedIntValue = 8;
            var expectedBoolValue = false;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 > {expectedIntValue}; boolProperty1 > {expectedBoolValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(GreaterThanFilter<int>));
            ((GreaterThanFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedIntValue);

            query.TryGetFilters(m => m.BoolProperty1, out var boolFilters).Should().BeTrue();

            boolFilters.Count().Should().Be(1);
            boolFilters.ElementAt(0).GetType().Should().Be(typeof(GreaterThanFilter<bool>));
            ((GreaterThanFilter<bool>)boolFilters.ElementAt(0)).Value.Should().Be(expectedBoolValue);
        }

        [Fact]
        public void GreaterThanOrEqualFilterTest()
        {
            var expectedIntValue = 8;
            var expectedBoolValue = false;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 >= {expectedIntValue}; boolProperty1 >= {expectedBoolValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(GreaterThanOrEqualsFilter<int>));
            ((GreaterThanOrEqualsFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedIntValue);

            query.TryGetFilters(m => m.BoolProperty1, out var boolFilters).Should().BeTrue();

            boolFilters.Count().Should().Be(1);
            boolFilters.ElementAt(0).GetType().Should().Be(typeof(GreaterThanOrEqualsFilter<bool>));
            ((GreaterThanOrEqualsFilter<bool>)boolFilters.ElementAt(0)).Value.Should().Be(expectedBoolValue);
        }

        [Fact]
        public void ContainsFilterTest()
        {
            var expectedStringValue = "test-string";

            var queryParams = new QueryParams
            {
                Filter = $"stringProperty1 _=_ {expectedStringValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(ContainsFilter));
            ((ContainsFilter)stringFilters.ElementAt(0)).Value.Should().Be(expectedStringValue);
        }

        [Fact]
        public void ApplyContainsFilterToNonStringTypeShouldThrowAndExceptionTest()
        {
            var expectedIntValue = 8;

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 _=_ {expectedIntValue}"
            };

            var act = () =>
            {
                var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);
            };

            act.Should().Throw<QurlException>();
        }

        [Fact]
        public void StartsWithFilterTest()
        {
            var expectedStringValue = "test-string";

            var queryParams = new QueryParams
            {
                Filter = $"stringProperty1 =_ {expectedStringValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(StartsWithFilter));
            ((StartsWithFilter)stringFilters.ElementAt(0)).Value.Should().Be(expectedStringValue);
        }

        [Fact]
        public void EndsWithFilterTest()
        {
            var expectedStringValue = "test-string";

            var queryParams = new QueryParams
            {
                Filter = $"stringProperty1 _= {expectedStringValue}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(EndsWithFilter));
            ((EndsWithFilter)stringFilters.ElementAt(0)).Value.Should().Be(expectedStringValue);
        }

        [Fact]
        public void InFilterTest()
        {
            var expectedIntValues = new[] { 3, 8, 15 };
            var expectedStringValues = new[] { "abc", "d" };

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 [] {string.Join(',', expectedIntValues)}; stringProperty1 [] {string.Join(',', expectedStringValues)}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(InFilter<int>));
            ((InFilter<int>)intFilters.ElementAt(0)).Values.Should().BeEquivalentTo(expectedIntValues);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(InFilter<string>));
            ((InFilter<string>)stringFilters.ElementAt(0)).Values.Should().BeEquivalentTo(expectedStringValues);
        }

        [Fact]
        public void NotInFilterTest()
        {
            var expectedIntValues = new[] { 3, 8, 15 };
            var expectedStringValues = new[] { "abc", "d" };

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 ![] {string.Join(',', expectedIntValues)}; stringProperty1 ![] {string.Join(',', expectedStringValues)}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(NotInFilter<int>));
            ((NotInFilter<int>)intFilters.ElementAt(0)).Values.Should().BeEquivalentTo(expectedIntValues);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(NotInFilter<string>));
            ((NotInFilter<string>)stringFilters.ElementAt(0)).Values.Should().BeEquivalentTo(expectedStringValues);
        }

        [Fact]
        public void FromToFilterTest()
        {
            var intFromExpected = 5;
            var intToExpected = 15;

            var stringFromExpected = "a";
            var stringToExpected = "x";

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 <-> {intFromExpected},{intToExpected}; stringProperty1 <-> {stringFromExpected},{stringToExpected}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(FromToFilter<int>));
            ((FromToFilter<int>)intFilters.ElementAt(0)).From.Should().Be(intFromExpected);
            ((FromToFilter<int>)intFilters.ElementAt(0)).To.Should().Be(intToExpected);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(FromToFilter<string>));
            ((FromToFilter<string>)stringFilters.ElementAt(0)).From.Should().Be(stringFromExpected);
            ((FromToFilter<string>)stringFilters.ElementAt(0)).To.Should().Be(stringToExpected);
        }

        [Theory]
        [InlineData("value", "value")]
        [InlineData("  value ", "value")]
        [InlineData("'value'", "value")]
        [InlineData("'test value'", "test value")]
        [InlineData(" 'test value'   ", "test value")]
        [InlineData("'  test value '", "  test value ")]
        [InlineData("'null'", "null")]
        [InlineData("null", default(string))]
        public void EqualsFilterStringPropertyTest(string value, string expectedValue)
        {
            var queryParams = new QueryParams
            {
                Filter = $"stringProperty1 == {value}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.StringProperty1, out var stringFilters).Should().BeTrue();

            stringFilters.Count().Should().Be(1);
            stringFilters.ElementAt(0).GetType().Should().Be(typeof(EqualsFilter<string>));
            ((EqualsFilter<string>)stringFilters.ElementAt(0)).Value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("  2 ", 2)]
        [InlineData("'3'", 3)]
        [InlineData("' 4 '", 4)]
        [InlineData("null", default(int))]
        public void EqualsFilterIntPropertyTest(string value, int expectedValue)
        {
            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == {value}"
            };

            var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);

            query.TryGetFilters(m => m.IntProperty1, out var intFilters).Should().BeTrue();

            intFilters.Count().Should().Be(1);
            intFilters.ElementAt(0).GetType().Should().Be(typeof(EqualsFilter<int>));
            ((EqualsFilter<int>)intFilters.ElementAt(0)).Value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("'null'")]
        public void EqualsFilterIntPropertyWithInvalidValueShouldThrowAnExceptionTest(string value)
        {
            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == {value}"
            };

            var act = () =>
            {
                var query = _queryBuilder.CreateQuery<TestModel1>(queryParams);
            };

            act.Should().Throw<QurlFormatException>();
        }

        [Fact]
        public void IgnoresPropertiesShouldNotBeIncludedAsFilters()
        {
            var queryParams = new QueryParams
            {
                Filter = "stringProperty1 == testValue"
            };

            var query = _queryBuilder.CreateQuery<TestModel2>(queryParams);

            query.TryGetFilters(m => m.StringProperty1, out _).Should().BeFalse();
        }

        [Fact]
        public void IgnoresPropertiesShouldNotBeIncludedAsOrderBy()
        {
            var queryParams = new QueryParams
            {
                OrderBy = "stringProperty1"
            };

            var query = _queryBuilder.CreateQuery<TestModel2>(queryParams);

            query.OrderBy.Any(o => o.PropertyName.Equals(nameof(TestModel2.StringProperty1), StringComparison.InvariantCultureIgnoreCase))
                .Should().BeFalse();
        }

        [Fact]
        public void QueryOptionAttrMapParamPropertyTest()
        {
            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == 8; string-property != testValue"
            };

            var query = _queryBuilder.CreateQuery<TestModel3>(queryParams);

            query.TryGetFilters(p => p.IntProperty1, out _).Should().BeTrue();

            query.TryGetFilters(p => p.StringProperty1, out _).Should().BeTrue();
        }

        [Fact]
        public void QueryOptionAttrMapModelPropertyTest()
        {
            var doublePropExpectedModelName = "RealDoubleProperty1";

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == 8; doubleProperty1 != 55"
            };

            var query = _queryBuilder.CreateQuery<TestModel3>(queryParams);

            query.TryGetFilters(p => p.IntProperty1, out _).Should().BeTrue();

            query.TryGetFilters(p => p.DoubleProperty1, out var doublePropFilters).Should().BeTrue();

            doublePropFilters.Count().Should().Be(1);
            doublePropFilters.First().ModelPropertyName.Should().Be(doublePropExpectedModelName);
        }

        [Fact]
        public void QueryOptionIgnoreNotSortablePropertyTest()
        {
            var expectedOrderByProperty = "IntProperty1";

            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == 8; doubleProperty1 != 55",
                OrderBy = "intProperty1, dateTimeProperty1"
            };

            var query = _queryBuilder.CreateQuery<TestModel3>(queryParams);

            query.OrderBy.Count().Should().Be(1);
            query.OrderBy.First().PropertyName.Should().Be(expectedOrderByProperty);
        }

        [Fact]
        public void QueryOptionHandleCustomFilterPropertyTest()
        {
            var queryParams = new QueryParams
            {
                Filter = $"intProperty1 == 8; enumProperty1 != value2"
            };

            var query = _queryBuilder.CreateQuery<TestModel3>(queryParams);

            query.Filters.Count().Should().Be(2);

            query.TryGetFilters(p => p.IntProperty1, out _).Should().BeTrue();

            query.TryGetFilters(p => p.EnumProperty1, out var enumPropFilters).Should().BeTrue();

            enumPropFilters.Count().Should().Be(1);

            enumPropFilters.First()!.CustomFiltering.Should().BeTrue();
        }

    }
}
