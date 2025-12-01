namespace BlogPlatform.Shared.Common.Models
{
    public class EntityWithCount<T> where T : notnull
    {
        public T Entity { get; set; } = default!;
        public long TotalCount { get; set; }
    }
}
