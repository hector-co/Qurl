namespace Qurl.Tests
{
    public class Class1
    {
        [Fact]
        public void T1()
        {
            var fr = new FilterFactory();
            var qh = new QueryHelper();
            var qb = new QueryBuilder(fr, qh);

            var model = new QueryModel
            {
                Filter = "prop1 == 25"
            };

            var query = qb.CreateQuery<SampleObject>(model);
        }
    }
}
