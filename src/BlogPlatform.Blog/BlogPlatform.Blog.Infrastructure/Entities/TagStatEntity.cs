namespace BlogPlatform.Blog.Infrastructure.Entities
{
    internal class TagStatEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PostsCount { get; set; }
    }
}
