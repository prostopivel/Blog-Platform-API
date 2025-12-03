using BlogPlatform.Analytics.Core.Models;

namespace BlogPlatform.Analytics.Core.Interfaces.Services
{
    public interface IIntervalService
    {
        IntervalItem<T> SplitListByIntervals<T, K>(List<K> items,
            DateTime startDate, DateTime endDate, Interval interval,
            Func<K, DateTime> getCreatedAt, Func<K, T> converter);
    }
}
