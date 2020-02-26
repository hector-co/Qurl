using System.Collections.Generic;
using System.Text;

namespace Qurl.Dapper
{
    public class QueryParts
    {
        public string Fields { get; set; }
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public string Filters { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string Sort { get; set; }
        public string Paging { get; set; }

        public string GetSqlQuery(bool includeSortAndPaging = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"SELECT {Fields}");
            sb.AppendLine($"FROM {TableName} {(string.IsNullOrEmpty(TableAlias) ? "" : TableAlias)}");
            var orderBy = includeSortAndPaging ? GetOrderByString() : "";
            if (!string.IsNullOrEmpty(Filters))
            {
                sb.AppendLine($"WHERE {Filters}");
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                sb.AppendLine(orderBy);
            }
            return sb.ToString();
        }

        public string GetSqlCountQuery(bool includeSortAndPaging = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"SELECT COUNT(*)");
            sb.AppendLine($"FROM {TableName} {(string.IsNullOrEmpty(TableAlias) ? "" : TableAlias)}");
            var orderBy = includeSortAndPaging ? GetOrderByString() : "";
            if (!string.IsNullOrEmpty(Filters))
            {
                sb.AppendLine($"WHERE {Filters}");
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                sb.AppendLine(orderBy);
            }
            return sb.ToString();
        }

        public string GetOrderByString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Sort))
            {
                sb.AppendLine($"ORDER BY {Sort}");
            }
            if (!string.IsNullOrEmpty(Paging))
            {
                sb.AppendLine(Paging);
            }
            return sb.ToString();
        }
    }
}
