namespace BlogPlatform.Analytics.Core.Models
{
    public record IdItem
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
