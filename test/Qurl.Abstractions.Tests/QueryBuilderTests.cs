using FluentAssertions;
using Qurl.Abstractions.Exceptions;
using System;
using Xunit;

namespace Qurl.Abstractions.Tests
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

            query.Page.Should().Be(0);
            query.PageSize.Should().Be(0);

            query.Sorts.Should().NotBeNull();
            query.Sorts.Count.Should().Be(1);
            query.Sorts[0].property.Should().BeNull();
            query.Sorts[0].direction.Should().Be(SortDirection.Ascending);

            query.ExtraFilters.Should().NotBeNull();
            query.ExtraFilters.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("page=4", 4, 0)]
        [InlineData("pageSize=10", 0, 10)]
        [InlineData("page=1&pageSize=5", 1, 5)]
        [InlineData("page=2&pageSize=10", 2, 10)]
        public void MapPagingTest(string queryString, int pageExpected, int pageSizeExpected)
        {
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);
            query.Page.Should().Be(pageExpected);
            query.PageSize.Should().Be(pageSizeExpected);
        }

        [Theory]
        [InlineData("sort=id,-name,description", new[] { "id", "name", "description" }, new[] { SortDirection.Ascending, SortDirection.Descending, SortDirection.Ascending })]
        [InlineData("sort=-prop1,-prop2,-prop3", new[] { "prop1", "prop2", "prop3" }, new[] { SortDirection.Descending, SortDirection.Descending, SortDirection.Descending })]
        public void MapSortingTest(string queryString, string[] properties, SortDirection[] directions)
        {
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);
            query.Sorts.Count.Should().Be(properties.Length);
            for (var i = 0; i < query.Sorts.Count; i++)
            {
                query.Sorts[i].property.Should().Be(properties[i]);
                query.Sorts[i].direction.Should().Be(directions[i]);
            }
        }

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

            var queryString = $"id[bt]={idFromExpected},{idToExpected}&name[bt]={nameFromExpected},{nameToExpected}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(BetweenFilterProperty<int>));
            ((BetweenFilterProperty<int>)query.Filter.Id).From.IsSet.Should().Be(true);
            ((BetweenFilterProperty<int>)query.Filter.Id).From.Value.Should().Be(idFromExpected);
            ((BetweenFilterProperty<int>)query.Filter.Id).To.IsSet.Should().Be(true);
            ((BetweenFilterProperty<int>)query.Filter.Id).To.Value.Should().Be(idToExpected);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(BetweenFilterProperty<string>));
            ((BetweenFilterProperty<string>)query.Filter.Name).From.IsSet.Should().Be(true);
            ((BetweenFilterProperty<string>)query.Filter.Name).From.Value.Should().Be(nameFromExpected);
            ((BetweenFilterProperty<string>)query.Filter.Name).To.IsSet.Should().Be(true);
            ((BetweenFilterProperty<string>)query.Filter.Name).To.Value.Should().Be(nameToExpected);

            query.Filter.Active.Should().BeNull();
        }

        [Fact]
        public void MapUncompleteBetweenPropertyTest()
        {
            var idToExpected = 15;

            var nameFromExpected = "a";

            var queryString = $"id[bt]=,{idToExpected}&name[bt]={nameFromExpected}";
            var query = (Query<TestFilter>)QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            query.Filter.Id.Should().NotBeNull();
            query.Filter.Id.GetType().Should().Be(typeof(BetweenFilterProperty<int>));
            ((BetweenFilterProperty<int>)query.Filter.Id).From.IsSet.Should().Be(false);
            ((BetweenFilterProperty<int>)query.Filter.Id).To.IsSet.Should().Be(true);
            ((BetweenFilterProperty<int>)query.Filter.Id).To.Value.Should().Be(idToExpected);

            query.Filter.Name.Should().NotBeNull();
            query.Filter.Name.GetType().Should().Be(typeof(BetweenFilterProperty<string>));
            ((BetweenFilterProperty<string>)query.Filter.Name).From.IsSet.Should().Be(true);
            ((BetweenFilterProperty<string>)query.Filter.Name).From.Value.Should().Be(nameFromExpected);
            ((BetweenFilterProperty<string>)query.Filter.Name).To.IsSet.Should().Be(false);

            query.Filter.Active.Should().BeNull();
        }

        [Theory]
        [InlineData("page", "")]
        [InlineData("page", "pageVal")]
        [InlineData("pageSize", "")]
        [InlineData("pageSize", "false")]
        [InlineData("id", "")]
        [InlineData("id", "!0")]
        [InlineData("id", "2,a")]
        [InlineData("active", "!true")]
        [InlineData("active", "5")]
        [InlineData("active", "true,2")]
        public void ThrowExceptionForInvalidParameterTest(string paramName, string value)
        {
            var queryString = $"{paramName}={value}";
            Action buildQuery = () => QueryBuilder.FromQueryString(typeof(Query<TestFilter>), queryString);

            buildQuery.Should().Throw<QurlParameterFormatException>().WithMessage(paramName);
        }
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
    }

    public class TestFilterEquals : TestFilter
    {
        public EqualsFilterProperty<int> EqProperty { get; set; }
    }

    public class DerivedQueryType : Query<TestFilter>
    {

    }
}