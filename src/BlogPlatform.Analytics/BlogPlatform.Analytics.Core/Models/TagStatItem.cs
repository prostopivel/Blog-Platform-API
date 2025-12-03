namespace BlogPlatform.Analytics.Core.Models
{
    public record TagStatItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PostsCount { get; set; }
    }
}
