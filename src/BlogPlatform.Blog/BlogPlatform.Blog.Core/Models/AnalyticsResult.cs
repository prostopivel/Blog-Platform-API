using BlogPlatform.Shared.Grpc.Models;

namespace BlogPlatform.Blog.Core.Models
{
    public class AnalyticsResult
    {
        public IEnumerable<PostsByDateItem> PostsByDateItems { get; set; } = [];
        public IEnumerable<PostIdByDateItem> CommentsByDateItems { get; set; } = [];
        public IEnumerable<PostIdByDateItem> LikesByDateItems { get; set; } = [];

        public AnalyticsResult()
        {
        }

        public AnalyticsResult(IEnumerable<PostsByDateItem> postsByDateItems,
            IEnumerable<PostIdByDateItem> commentsByDateItems,
            IEnumerable<PostIdByDateItem> likesByDateItems)
        {
            PostsByDateItems = postsByDateItems;
            CommentsByDateItems = commentsByDateItems;
            LikesByDateItems = likesByDateItems;
        }
    }
}
