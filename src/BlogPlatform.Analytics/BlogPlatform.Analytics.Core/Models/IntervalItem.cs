using BlogPlatform.Analytics.Core.Interfaces;

namespace BlogPlatform.Analytics.Core.Models
{
    public record IntervalItem<T>
    {
        public List<T>[] Items { get; set; } = [];
        public Interval Interval { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
