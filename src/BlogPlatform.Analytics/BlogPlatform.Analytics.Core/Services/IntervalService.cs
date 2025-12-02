using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Models;

namespace BlogPlatform.Analytics.Core.Services
{
    public class IntervalService : IIntervalService
    {
        public IntervalItem<T> SplitListByIntervals<T, K>(List<K> items,
            DateTime startDate, DateTime endDate, Interval interval,
            Func<K, DateTime> getCreatedAt, Func<K, T> converter)
        {
            int intervalHours = (int)interval;
            double totalHours = (endDate - startDate).TotalHours;
            var intervalsCount = (int)Math.Ceiling(totalHours / intervalHours);

            var splitedItems = new IntervalItem<T>()
            {
                Items = new List<T>[intervalsCount],
                Interval = interval,
                StartDate = startDate,
                EndDate = endDate
            };

            for (int i = 0; i < intervalsCount; i++)
            {
                splitedItems.Items[i] = [];
            }

            foreach (var item in items)
            {
                DateTime createdAt = getCreatedAt(item);
                if (createdAt < startDate || createdAt > endDate)
                    continue;

                double hoursFromStart = (createdAt - startDate).TotalHours;
                int index = (int)(hoursFromStart / intervalHours);

                if (index >= 0 && index < intervalsCount)
                {
                    splitedItems.Items[index].Add(converter(item));
                }
            }

            return splitedItems;
        }
    }
}
