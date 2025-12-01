namespace BlogPlatform.Blog.API.DTOs
{
    public record UpdatePostRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = [];
    }
}
