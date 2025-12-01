namespace BlogPlatform.Blog.IntegrationTests.DTOs
{
    public record LikeResult
    {
        public bool Liked { get; set; }
        public int LikesCount { get; set; }
    }
}
