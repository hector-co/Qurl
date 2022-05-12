using FluentAssertions;
using Qurl.Dapper;
using Xunit;

namespace Qurl.Tests
{
    public class DapperTests
    {
        [Fact]
        public void TestEqualsFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] = {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new EqualsFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestEqualsFilterWithTableSchema()
        {
            const int prop1FilterValue = 2;
            const string tableSchema = "dbo";
            const string tableName = "SampleObject";
            var expectedTableName = $"[{tableSchema}].[{tableName}]";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new EqualsFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts(tableName, tableSchema: tableSchema);

            queryParts.TableName.Should().Be(expectedTableName);
        }

        [Fact]
        public void TestEqualsFilterWithTableAlias()
        {
            const int prop1FilterValue = 2;
            const string tableAlias = "t0";
            var expectedTableAlias = $"[{tableAlias}]";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new EqualsFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject", tableAlias: tableAlias);

            queryParts.TableAlias.Should().Be(expectedTableAlias);
        }

        [Fact]
        public void TestNotEqualsFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] <> {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new NotEqualsFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestLessThanFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] < {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new LessThanFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestLessThanOrEqualFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] <= {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new LessThanOrEqualFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestGreatherThanFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] > {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new GreaterThanFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestGreatherThanOrEqualFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] >= {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new GreaterThanOrEqualFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestContainsFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] LIKE CONCAT('%', { expectedParamterName}, '%')";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new ContainsFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestStartsWithFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] LIKE CONCAT({ expectedParamterName}, '%')";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new StartsWithFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestEndsWithFilter()
        {
            const int prop1FilterValue = 2;
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] LIKE CONCAT('%', { expectedParamterName})";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new EndsWithFilterProperty<int>
            {
                Value = prop1FilterValue
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().Be(prop1FilterValue);
        }

        [Fact]
        public void TestInFilter()
        {
            var prop1FilterValues = new[] { 2, 4 };
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] IN {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new InFilterProperty<int>
            {
                Values = prop1FilterValues
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().BeEquivalentTo(prop1FilterValues);
        }

        [Fact]
        public void TestNotInFilter()
        {
            var prop1FilterValues = new[] { 2, 4 };
            const string column = "Prop1";
            var expectedParamterName = $"@{column}";
            var expectedFilter = $"[{column}] NOT IN {expectedParamterName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new NotInFilterProperty<int>
            {
                Values = prop1FilterValues
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterName);
            queryParts.Parameters[expectedParamterName].Should().BeEquivalentTo(prop1FilterValues);
        }

        [Fact]
        public void TestRangeFilter()
        {
            const int prop1FilterFromValue = 2;
            const int prop1FilterToValue = 6;
            const string column = "Prop1";
            string expectedParamterFromName = $"@{column}From";
            string expectedParamterToName = $"@{column}To";
            var expectedFilter = $"[{column}] >= {expectedParamterFromName} AND [{column}] <= {expectedParamterToName}";
            var query = new Query<SampleObjectFilter>();
            query.Filter.Prop1 = new RangeFilterProperty<int>
            {
                From = new Seteable<int>(prop1FilterFromValue),
                To = new Seteable<int>(prop1FilterToValue)
            };

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Filters.Should().Be(expectedFilter);
            queryParts.Parameters.Should().NotBeEmpty();
            queryParts.Parameters.Should().ContainKey(expectedParamterFromName);
            queryParts.Parameters[expectedParamterFromName].Should().Be(prop1FilterFromValue);
            queryParts.Parameters.Should().ContainKey(expectedParamterToName);
            queryParts.Parameters[expectedParamterToName].Should().Be(prop1FilterToValue);
        }

        [Fact]
        public void TestEmptySort()
        {
            var query = new Query<SampleObjectFilter>();

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Sort.Should().BeEmpty();
        }

        [Fact]
        public void TestSortWithoutPaging()
        {
            var query = new Query<SampleObjectFilter>();
            const string ExpectedSort = "[Prop1] DESC";
            query.Sort.Add(new SortValue("Prop1", SortDirection.Descending));

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Sort.Should().Be(ExpectedSort);
        }

        [Fact]
        public void PagingWithoutSortShouldBeEmpty()
        {
            var query = new Query<SampleObjectFilter>();
            query.Limit = 10;
            query.Offset = 20;

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Paging.Should().BeEmpty();
        }

        [Fact]
        public void TestSortAndPaging()
        {
            var query = new Query<SampleObjectFilter>();
            const string ExpectedSort = "[Prop1] DESC";
            const string ExpectedPaging = "OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY";
            query.Sort.Add(new SortValue("Prop1", SortDirection.Descending));
            query.Limit = 10;
            query.Offset = 20;

            var queryParts = query.GetQueryParts("SampleObject");

            queryParts.Sort.Should().Be(ExpectedSort);
            queryParts.Paging.Should().Be(ExpectedPaging);
        }

    }
}
