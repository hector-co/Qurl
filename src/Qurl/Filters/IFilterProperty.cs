namespace Qurl.Filters
{
    public interface IFilterProperty : IFilter
    {
        string PropertyName { get; }
        string ModelPropertyName { get; }
        bool CustomFiltering { get; }
        void SetValueFromString(params string?[] values);

        void SetOptions(string propertyName, string modelPropertyName, bool isCustomFilter);
    }
}
