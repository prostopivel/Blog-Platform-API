namespace BlogPlatform.Blog.Infrastructure.Entities
{
    internal class PostsByDateItemEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
    }
}
