using BlogPlatform.Analytics.Core.Interfaces;

namespace BlogPlatform.Analytics.Core.Models
{
    public record PostItem
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
    }
}
