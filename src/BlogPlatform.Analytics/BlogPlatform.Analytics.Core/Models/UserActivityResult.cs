namespace BlogPlatform.Analytics.Core.Models
{
    public record UserActivityResult
    {
        public IntervalItem<PostItem> PostItems { get; set; } = new();
        public IntervalItem<IdItem> CommentItems { get; set; } = new();
        public IntervalItem<IdItem> LikeItems { get; set; } = new();
    }
}
