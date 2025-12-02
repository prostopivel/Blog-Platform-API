using BlogPlatform.Shared.Grpc.Models;

namespace BlogPlatform.Blog.Core.Models
{
    public class AnalyticsResult
    {
        public IEnumerable<PostsByDateItem> PostsByDateItems { get; set; } = [];
        public IEnumerable<IdsByDateItem> CommentsByDateItems { get; set; } = [];
        public IEnumerable<IdsByDateItem> LikesByDateItems { get; set; } = [];

        public AnalyticsResult()
        {
        }

        public AnalyticsResult(IEnumerable<PostsByDateItem> postsByDateItems,
            IEnumerable<IdsByDateItem> commentsByDateItems,
            IEnumerable<IdsByDateItem> likesByDateItems)
        {
            PostsByDateItems = postsByDateItems;
            CommentsByDateItems = commentsByDateItems;
            LikesByDateItems = likesByDateItems;
        }
    }
}
