namespace BlogPlatform.Blog.API.DTOs
{
    public class CreateCommentRequest
    {
        public Guid PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
