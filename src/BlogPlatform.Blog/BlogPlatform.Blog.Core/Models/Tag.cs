namespace BlogPlatform.Blog.Core.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Tag()
        {
        }

        public Tag(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
