namespace BlogPlatform.Blog.Core.Models
{
    public class LikeResult
    {
        public bool Liked { get; set; }
        public int LikesCount { get; set; }

        public LikeResult()
        {
        }

        public LikeResult(bool liked, int likesCount)
        {
            Liked = liked;
            LikesCount = likesCount;
        }
    }
}
