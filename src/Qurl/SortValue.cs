namespace Qurl
{
    public class SortValue
    {
        public SortValue(string propertyName = default, SortDirection sortDirection = default)
        {
            PropertyName = propertyName;
            SortDirection = sortDirection;
        }

        public string PropertyName { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}
