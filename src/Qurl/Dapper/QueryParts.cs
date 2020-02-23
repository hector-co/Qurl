using System.Collections.Generic;
using System.Text;

namespace Qurl.Dapper
{
    public struct QueryParts
    {
        public string Fields { get; set; }
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public string Filter { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string Sort { get; set; }
        public string Paging { get; set; }

        public string GetSqlQuery(bool includeSortAndPaging = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"SELECT {Fields}");
            sb.AppendLine($"FROM {TableName} {(string.IsNullOrEmpty(TableAlias) ? "" : TableAlias)}");
            if (!string.IsNullOrEmpty(Filter))
            {
                sb.AppendLine($"WHERE {Filter}");
            }
            if (!string.IsNullOrEmpty(Sort) && includeSortAndPaging)
            {
                sb.AppendLine($"ORDER BY {Sort}");
            }
            if (!string.IsNullOrEmpty(Paging) && includeSortAndPaging)
            {
                sb.AppendLine(Paging);
            }
            return sb.ToString(); ;
        }
    }
}
