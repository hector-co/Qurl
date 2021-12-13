using FluentAssertions;
using Qurl.Exceptions;
using System;
using System.Linq;
using Xunit;

namespace Qurl.Tests
{
    public class QueryBuilderTests
    {
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(NonQueryType))]
        [InlineData(typeof(NonQueryGenericType<TestFilter>))]
        public void BuildFromNonQueryTypesShouldFailTest(Type nonQueryType)
        {
            Action buildQuery = () => QueryBuilder.FromQueryString(nonQueryType, "");

            buildQuery.Should().Throw<QurlException>();
        }

        [Fact]
        public void BuildFromQueryTypeTest()
        {
            var query = QueryBuilder.FromQueryString(typeof(Query<TestFilter>), "");
            query.Should().NotBeNull();
            query.GetType().Should().Be(typeof(Query<TestFilter>));
        }

        [Fact]
        public void BuildFromDerivedQueryTypeTest()
        {
            var query = QueryBuilder.FromQueryString(typeof(DerivedQueryType), "");
            query.Should().NotBeNull();
            query.GetType().Should().Be(typeof(DerivedQueryType));
        }

        [Fact]
        public void BuildFromEmptyQueryStringTest()
        {
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), "");

            query.Filter.Should().NotBeNull();
            query.Filter.Id.Should().BeNull();
            query.Filter.Name.Should().BeNull();
            query.Filter.Active.Should().BeNull();

            query.Offset.Should().Be(0);
            query.Limit.Should().Be(0);

            query.Sort.Should().NotBeNull();
            query.Sort.Count.Should().Be(0);

            query.GetExtraFilters().Should().NotBeNull();
            query.GetExtraFilters().Count.Should().Be(0);
        }

        [Fact]
        public void BuildFromEmptyQueryStringUsingGenericsTest()
        {
            var query = QueryBuilder.FromQueryString<Query<TestFilter>>("");

            query.Filter.Should().NotBeNull();
            query.Filter.Id.Should().BeNull();
            query.Filter.Name.Should().BeNull();
            query.Filter.Active.Should().BeNull();

            query.Offset.Should().Be(0);
            query.Limit.Should().Be(0);

            query.Sort.Should().NotBeNull();
            query.Sort.Count.Should().Be(0);

            query.GetExtraFilters().Should().NotBeNull();
            query.GetExtraFilters().Count.Should().Be(0);
        }

        [Fact]
        public void TestDefaultSort()
        {
            var query = new SampleObjectQueryWithDefaultSort();
            query.GetEvalSorts().Count.Should().Be(1);
            query.GetEvalSorts().First().PropertyName.ToLower().Should().Be("prop1");
            query.GetEvalSorts().First().SortDirection.Should().Be(SortDirection.Descending);
        }

        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("offset=4", 4, 0)]
        [InlineData("limit=10", 0, 10)]
        [InlineData("offset=1&limit=5", 1, 5)]
        [InlineData("offset=2&limit=10", 2, 10)]
        public void MapPagingTest(string queryString, int pageExpected, int pageSizeExpected)
        {
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);
            query.Offset.Should().Be(pageExpected);
            query.Limit.Should().Be(pageSizeExpected);
        }

        [Theory]
        [InlineData("sort=id,-name,description", new[] { "id", "name", "description" }, new[] { SortDirection.Ascending, SortDirection.Descending, SortDirection.Ascending })]
        [InlineData("sort=-prop1,-prop2,-prop3", new[] { "prop1", "prop2", "prop3" }, new[] { SortDirection.Descending, SortDirection.Descending, SortDirection.Descending })]
        public void MapSortingTest(string queryString, string[] properties, SortDirection[] directions)
        {
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);
            query.Sort.Count.Should().Be(properties.Length);
            for (var i = 0; i < query.Sort.Count; i++)
            {
                query.Sort[i].PropertyName.Should().Be(properties[i]);
                query.Sort[i].SortDirection.Should().Be(directions[i]);
            }
        }

        [Theory]
        [InlineData("fields=id,name", new[] { "id", "name" })]
        [InlineData("fields=prop1,prop3,prop4.name", new[] { "prop1", "prop3", "prop4.name" })]
        public void MapFieldsTest(string queryString, string[] properties)
        {
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);
            query.Fields.Should().BeEquivalentTo(properties);
        }

        #region LHS
        [Fact]
        public void MapEqualsPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[eq]={idExpectedValue}&active={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(EqualsFilterProperty<int>));
            ((EqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(EqualsFilterProperty<bool>));
            ((EqualsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapNotEqualsPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[neq]={idExpectedValue}&active[neq]={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(NotEqualsFilterProperty<int>));
            ((NotEqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(NotEqualsFilterProperty<bool>));
            ((NotEqualsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapLessThanPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[lt]={idExpectedValue}&active[lt]={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(LessThanFilterProperty<int>));
            ((LessThanFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(LessThanFilterProperty<bool>));
            ((LessThanFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapLessThanOrEqualPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[lte]={idExpectedValue}&active[lte]={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(LessThanOrEqualFilterProperty<int>));
            ((LessThanOrEqualFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(LessThanOrEqualFilterProperty<bool>));
            ((LessThanOrEqualFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapGreaterThanPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[gt]={idExpectedValue}&active[gt]={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(GreaterThanFilterProperty<int>));
            ((GreaterThanFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(GreaterThanFilterProperty<bool>));
            ((GreaterThanFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapGreaterThanOrEqualPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[gte]={idExpectedValue}&active[gte]={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(GreaterThanOrEqualFilterProperty<int>));
            ((GreaterThanOrEqualFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(GreaterThanOrEqualFilterProperty<bool>));
            ((GreaterThanOrEqualFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapContainsPropertyTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id[ct]={idExpectedValue}&active[ct]={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(ContainsFilterProperty<int>));
            ((ContainsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(ContainsFilterProperty<bool>));
            ((ContainsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapInPropertyTest()
        {
            var idExpectedValues = new[] { 3, 8, 15 };
            var nameExpectedValues = new[] { "abc", "d" };
            var activeExpectedValues = new bool[] { };

            var queryString = $"id[in]={string.Join(',', idExpectedValues)}&name[in]={string.Join(',', nameExpectedValues)}&active[in]={string.Join(',', activeExpectedValues)}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(InFilterProperty<int>));
            ((InFilterProperty<int>)query.Filter.Id).Values.Should().HaveSameCount(idExpectedValues);
            ((InFilterProperty<int>)query.Filter.Id).Values.Should().IntersectWith(idExpectedValues);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(InFilterProperty<string>));
            ((InFilterProperty<string>)query.Filter.Name).Values.Should().HaveSameCount(nameExpectedValues);
            ((InFilterProperty<string>)query.Filter.Name).Values.Should().IntersectWith(nameExpectedValues);

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(InFilterProperty<bool>));
            ((InFilterProperty<bool>)query.Filter.Active).Values.Should().HaveSameCount(activeExpectedValues);
        }

        [Fact]
        public void MapNotInPropertyTest()
        {
            var idExpectedValues = new[] { 3, 8, 15 };
            var nameExpectedValues = new[] { "abc", "d" };
            var activeExpectedValues = new bool[] { };

            var queryString = $"id[nin]={string.Join(',', idExpectedValues)}&name[nin]={string.Join(',', nameExpectedValues)}&active[nin]={string.Join(',', activeExpectedValues)}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(NotInFilterProperty<int>));
            ((NotInFilterProperty<int>)query.Filter.Id).Values.Should().HaveSameCount(idExpectedValues);
            ((NotInFilterProperty<int>)query.Filter.Id).Values.Should().IntersectWith(idExpectedValues);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(NotInFilterProperty<string>));
            ((NotInFilterProperty<string>)query.Filter.Name).Values.Should().HaveSameCount(nameExpectedValues);
            ((NotInFilterProperty<string>)query.Filter.Name).Values.Should().IntersectWith(nameExpectedValues);

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(NotInFilterProperty<bool>));
            ((NotInFilterProperty<bool>)query.Filter.Active).Values.Should().HaveSameCount(activeExpectedValues);
        }

        [Fact]
        public void MapBetweenPropertyTest()
        {
            var idFromExpected = 5;
            var idToExpected = 15;

            var nameFromExpected = "a";
            var nameToExpected = "x";

            var queryString = $"id[rng]={idFromExpected},{idToExpected}&name[rng]={nameFromExpected},{nameToExpected}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(RangeFilterProperty<int>));
            ((RangeFilterProperty<int>)query.Filter.Id).From.IsSet.Should().Be(true);
            ((RangeFilterProperty<int>)query.Filter.Id).From.Value.Should().Be(idFromExpected);
            ((RangeFilterProperty<int>)query.Filter.Id).To.IsSet.Should().Be(true);
            ((RangeFilterProperty<int>)query.Filter.Id).To.Value.Should().Be(idToExpected);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(RangeFilterProperty<string>));
            ((RangeFilterProperty<string>)query.Filter.Name).From.IsSet.Should().Be(true);
            ((RangeFilterProperty<string>)query.Filter.Name).From.Value.Should().Be(nameFromExpected);
            ((RangeFilterProperty<string>)query.Filter.Name).To.IsSet.Should().Be(true);
            ((RangeFilterProperty<string>)query.Filter.Name).To.Value.Should().Be(nameToExpected);

            query.Filter.Active.Should().BeNull();
        }

        [Fact]
        public void MapUncompleteBetweenPropertyTest()
        {
            var idToExpected = 15;

            var nameFromExpected = "a";

            var queryString = $"id[rng]=,{idToExpected}&name[rng]={nameFromExpected}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(RangeFilterProperty<int>));
            ((RangeFilterProperty<int>)query.Filter.Id).From.IsSet.Should().Be(false);
            ((RangeFilterProperty<int>)query.Filter.Id).To.IsSet.Should().Be(true);
            ((RangeFilterProperty<int>)query.Filter.Id).To.Value.Should().Be(idToExpected);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(RangeFilterProperty<string>));
            ((RangeFilterProperty<string>)query.Filter.Name).From.IsSet.Should().Be(true);
            ((RangeFilterProperty<string>)query.Filter.Name).From.Value.Should().Be(nameFromExpected);
            ((RangeFilterProperty<string>)query.Filter.Name).To.IsSet.Should().Be(false);

            query.Filter.Active.Should().BeNull();
        }

        [Theory]
        [InlineData("offset", "offset", "")]
        [InlineData("offset", "offset", "offsetVal")]
        [InlineData("limit", "limit", "")]
        [InlineData("limit", "limit", "false")]
        [InlineData("id", "id", "")]
        [InlineData("id[in]", "id", "a")]
        [InlineData("id", "id", "!0")]
        [InlineData("id", "id", "2,a")]
        [InlineData("active", "active", "!true")]
        [InlineData("active", "active", "5")]
        [InlineData("active[bt]", "active", "5")]
        [InlineData("active", "active", "true,2")]
        [InlineData("active[eq]", "active", "true,2")]
        public void ThrowExceptionForInvalidParameterTest(string paramName, string propName, string value)
        {
            var queryString = $"{paramName}={value}";
            Action buildQuery = () => QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            buildQuery.Should().Throw<QurlParameterFormatException>().WithMessage(propName);
        }

        [Fact]
        public void MapInPropertyFromUrlArray()
        {
            const string queryString = "id=1&id=2&id=3";
            const int expectedCount = 3;
            var expectedValues = new[] { 1, 2, 3 };
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(InFilterProperty<int>));
            ((InFilterProperty<int>)query.Filter.Id).Values.Count().Should().Be(expectedCount);
            ((InFilterProperty<int>)query.Filter.Id).Values.Should().Contain(expectedValues);
        }

        [Fact]
        public void MapNotInPropertyFromUrlArrayToSpecificPropertyType()
        {
            const string queryString = "tag=val1&tag=val2&tag=val3";
            const int expectedCount = 3;
            var expectedValues = new[] { "val1", "val2", "val3" };
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Tag.Should().NotBeNull();
            query.Filter.Tag.GetType().Should().Be(typeof(NotInFilterProperty<string>));
            query.Filter.Tag.Values.Count().Should().Be(expectedCount);
            query.Filter.Tag.Values.Should().Contain(expectedValues);
        }

        [Fact]
        public void MapToInheritedObjectProperties()
        {
            const string queryString = "id=1&name=testname&active=true&tag=val1&tag=val2&tag=val3";
            var query = (Query<TestFilterChild>)QueryBuilder.FromQueryString(typeof(Query<TestFilterChild>), queryString);

            query.Filter.Id.Should().NotBeNull();
            ((EqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(1);
            query.Filter.Name.Should().NotBeNull();
            ((EqualsFilterProperty<string>)query.Filter.Name).Value.Should().Be("testname");
            query.Filter.Tag.GetType().Should().Be(typeof(NotInFilterProperty<string>));
        }

        [Fact]
        public void MapInPropertyWithQuotes()
        {
            const string queryString = @"name[in]=""val1.1,val1.2"",val2, , val3 ,,val4";
            const int expectedCount = 6;
            var expectedValues = new[] { @"""val1.1,val1.2""", "val2", " ", " val3 ", "", "val4" };
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(InFilterProperty<string>));
            ((InFilterProperty<string>)query.Filter.Name).Values.Count().Should().Be(expectedCount);
            ((InFilterProperty<string>)query.Filter.Name).Values.Should().Contain(expectedValues);
        }

        [Fact]
        public void MapFilterPropertiesWithFilterPrefix()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"Filter.id[eq]={idExpectedValue}&filter.active={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(EqualsFilterProperty<int>));
            ((EqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(EqualsFilterProperty<bool>));
            ((EqualsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }
        #endregion

        #region RHS
        [Fact]
        public void MapEqualsPropertyRhsTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id=eq:{idExpectedValue}&active={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(EqualsFilterProperty<int>));
            ((EqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(EqualsFilterProperty<bool>));
            ((EqualsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapContainsPropertyRhsTest()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"id=ct:{idExpectedValue}&active=ct:{activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(ContainsFilterProperty<int>));
            ((ContainsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(ContainsFilterProperty<bool>));
            ((ContainsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }

        [Fact]
        public void MapInPropertyRhsTest()
        {
            var idExpectedValues = new[] { 3, 8, 15 };
            var nameExpectedValues = new[] { "abc", "d" };
            var activeExpectedValues = new bool[] { };

            var queryString = $"id=in:{string.Join(',', idExpectedValues)}&name=in:{string.Join(',', nameExpectedValues)}&active=in:{string.Join(',', activeExpectedValues)}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(InFilterProperty<int>));
            ((InFilterProperty<int>)query.Filter.Id).Values.Should().HaveSameCount(idExpectedValues);
            ((InFilterProperty<int>)query.Filter.Id).Values.Should().IntersectWith(idExpectedValues);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(InFilterProperty<string>));
            ((InFilterProperty<string>)query.Filter.Name).Values.Should().HaveSameCount(nameExpectedValues);
            ((InFilterProperty<string>)query.Filter.Name).Values.Should().IntersectWith(nameExpectedValues);

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(InFilterProperty<bool>));
            ((InFilterProperty<bool>)query.Filter.Active).Values.Should().HaveSameCount(activeExpectedValues);
        }

        [Fact]
        public void MapNotInPropertyRhsTest()
        {
            var idExpectedValues = new[] { 3, 8, 15 };
            var nameExpectedValues = new[] { "abc", "d" };
            var activeExpectedValues = new bool[] { };

            var queryString = $"id=nin:{string.Join(',', idExpectedValues)}&name=nin:{string.Join(',', nameExpectedValues)}&active=nin:{string.Join(',', activeExpectedValues)}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(NotInFilterProperty<int>));
            ((NotInFilterProperty<int>)query.Filter.Id).Values.Should().HaveSameCount(idExpectedValues);
            ((NotInFilterProperty<int>)query.Filter.Id).Values.Should().IntersectWith(idExpectedValues);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(NotInFilterProperty<string>));
            ((NotInFilterProperty<string>)query.Filter.Name).Values.Should().HaveSameCount(nameExpectedValues);
            ((NotInFilterProperty<string>)query.Filter.Name).Values.Should().IntersectWith(nameExpectedValues);

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(NotInFilterProperty<bool>));
            ((NotInFilterProperty<bool>)query.Filter.Active).Values.Should().HaveSameCount(activeExpectedValues);
        }

        [Fact]
        public void MapBetweenPropertyRhsTest()
        {
            var idFromExpected = 5;
            var idToExpected = 15;

            var nameFromExpected = "a";
            var nameToExpected = "x";

            var queryString = $"id=rng:{idFromExpected},{idToExpected}&name=rng:{nameFromExpected},{nameToExpected}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(RangeFilterProperty<int>));
            ((RangeFilterProperty<int>)query.Filter.Id).From.IsSet.Should().Be(true);
            ((RangeFilterProperty<int>)query.Filter.Id).From.Value.Should().Be(idFromExpected);
            ((RangeFilterProperty<int>)query.Filter.Id).To.IsSet.Should().Be(true);
            ((RangeFilterProperty<int>)query.Filter.Id).To.Value.Should().Be(idToExpected);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(RangeFilterProperty<string>));
            ((RangeFilterProperty<string>)query.Filter.Name).From.IsSet.Should().Be(true);
            ((RangeFilterProperty<string>)query.Filter.Name).From.Value.Should().Be(nameFromExpected);
            ((RangeFilterProperty<string>)query.Filter.Name).To.IsSet.Should().Be(true);
            ((RangeFilterProperty<string>)query.Filter.Name).To.Value.Should().Be(nameToExpected);

            query.Filter.Active.Should().BeNull();
        }

        [Fact]
        public void MapUncompleteBetweenPropertyRhsTest()
        {
            var idToExpected = 15;

            var nameFromExpected = "a";

            var queryString = $"id=rng:,{idToExpected}&name=rng:{nameFromExpected}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(RangeFilterProperty<int>));
            ((RangeFilterProperty<int>)query.Filter.Id).From.IsSet.Should().Be(false);
            ((RangeFilterProperty<int>)query.Filter.Id).To.IsSet.Should().Be(true);
            ((RangeFilterProperty<int>)query.Filter.Id).To.Value.Should().Be(idToExpected);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(RangeFilterProperty<string>));
            ((RangeFilterProperty<string>)query.Filter.Name).From.IsSet.Should().Be(true);
            ((RangeFilterProperty<string>)query.Filter.Name).From.Value.Should().Be(nameFromExpected);
            ((RangeFilterProperty<string>)query.Filter.Name).To.IsSet.Should().Be(false);

            query.Filter.Active.Should().BeNull();
        }

        [Theory]
        [InlineData("offset", "offset", "")]
        [InlineData("offset", "offset", "offsetVal")]
        [InlineData("limit", "limit", "")]
        [InlineData("limit", "limit", "false")]
        [InlineData("id", "id", "")]
        [InlineData("id", "id", "in:a")]
        [InlineData("id", "id", "!0")]
        [InlineData("id", "id", "2,a")]
        [InlineData("active", "active", "!true")]
        [InlineData("active", "active", "5")]
        [InlineData("active", "active", "bt:5")]
        [InlineData("active", "active", "true,2")]
        [InlineData("active", "active", "eq:true,2")]
        public void ThrowExceptionForInvalidParameterRhsTest(string paramName, string propName, string value)
        {
            var queryString = $"{paramName}={value}";
            Action buildQuery = () => QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            buildQuery.Should().Throw<QurlParameterFormatException>().WithMessage(propName);
        }

        [Fact]
        public void MapInPropertyFromUrlArrayRhs()
        {
            const string queryString = "id=1&id=2&id=3";
            const int expectedCount = 3;
            var expectedValues = new[] { 1, 2, 3 };
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(InFilterProperty<int>));
            ((InFilterProperty<int>)query.Filter.Id).Values.Count().Should().Be(expectedCount);
            ((InFilterProperty<int>)query.Filter.Id).Values.Should().Contain(expectedValues);
        }

        [Fact]
        public void MapNotInPropertyFromUrlArrayToSpecificPropertyTypeRhs()
        {
            const string queryString = "tag=val1&tag=val2&tag=val3";
            const int expectedCount = 3;
            var expectedValues = new[] { "val1", "val2", "val3" };
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Tag.Should().NotBeNull();
            query.Filter.Tag.GetType().Should().Be(typeof(NotInFilterProperty<string>));
            query.Filter.Tag.Values.Count().Should().Be(expectedCount);
            query.Filter.Tag.Values.Should().Contain(expectedValues);
        }

        [Fact]
        public void MapToInheritedObjectPropertiesRhs()
        {
            const string queryString = "id=1&name=testname&active=true&tag=val1&tag=val2&tag=val3";
            var query = (Query<TestFilterChild>)QueryBuilder.FromQueryString(typeof(Query<TestFilterChild>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            ((EqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(1);
            query.Filter.Name.Should().NotBeNull();
            ((EqualsFilterProperty<string>)query.Filter.Name).Value.Should().Be("testname");
            query.Filter.Tag.GetType().Should().Be(typeof(NotInFilterProperty<string>));
        }

        [Fact]
        public void MapInPropertyWithQuotesRhs()
        {
            const string queryString = @"name=in:""val1.1,val1.2"",val2, , val3 ,,val4";
            const int expectedCount = 6;
            var expectedValues = new[] { @"""val1.1,val1.2""", "val2", " ", " val3 ", "", "val4" };
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(InFilterProperty<string>));
            ((InFilterProperty<string>)query.Filter.Name).Values.Count().Should().Be(expectedCount);
            ((InFilterProperty<string>)query.Filter.Name).Values.Should().Contain(expectedValues);
        }

        [Fact]
        public void MapFilterPropertiesWithFilterPrefixRhs()
        {
            var idExpectedValue = 8;
            var activeExpectedValue = false;

            var queryString = $"Filter.id=eq:{idExpectedValue}&filter.active={activeExpectedValue}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString, mode: FilterMode.RHS);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(EqualsFilterProperty<int>));
            ((EqualsFilterProperty<int>)query.Filter.Id).Value.Should().Be(idExpectedValue);

            query.Filter.Name.Should().BeNull();

            query.Filter.Active.Should().NotBeNull();
            query.Filter.Active.GetType().Should().Be(typeof(EqualsFilterProperty<bool>));
            ((EqualsFilterProperty<bool>)query.Filter.Active).Value.Should().Be(activeExpectedValue);
        }
        #endregion
    }

    public class NonQueryType
    {
        FilterProperty<int> Id { get; set; }
    }

    public class NonQueryGenericType<T>
    {

    }

    public class TestFilter
    {
        public FilterProperty<int> Id { get; set; }
        public FilterProperty<string> Name { get; set; }
        public FilterProperty<bool> Active { get; set; }

        public NotInFilterProperty<string> Tag { get; set; }
    }

    public class TestFilterEquals : TestFilter
    {
        public EqualsFilterProperty<int> EqProperty { get; set; }
    }

    public class DerivedQueryType : Query<TestFilter>
    {

    }

    public class TestFilterChild : TestFilter
    {

    }
}
