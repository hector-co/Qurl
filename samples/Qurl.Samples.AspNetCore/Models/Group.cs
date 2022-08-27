namespace Qurl.Samples.AspNetCore.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}
