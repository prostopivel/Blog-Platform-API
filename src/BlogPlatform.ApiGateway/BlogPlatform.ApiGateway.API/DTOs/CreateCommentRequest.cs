namespace BlogPlatform.ApiGateway.API.DTOs
{
    public record CreateCommentRequest
    {
        public Guid PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
