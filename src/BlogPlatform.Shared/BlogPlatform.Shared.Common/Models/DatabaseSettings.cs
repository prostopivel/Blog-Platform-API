namespace BlogPlatform.Shared.Common.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; }
        public int ConnectionTimeout { get; set; }
    }
}
