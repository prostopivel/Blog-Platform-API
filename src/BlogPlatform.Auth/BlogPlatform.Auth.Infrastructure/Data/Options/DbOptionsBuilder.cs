using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Auth.Infrastructure.Data.Options
{
    public class DbOptionsBuilder
    {
        private readonly DatabaseSettings _options = new();

        public DbOptionsBuilder WithConnectionString(string connectionString)
        {
            _options.ConnectionString = connectionString;
            return this;
        }

        internal DatabaseSettings Build()
        {
            if (string.IsNullOrEmpty(_options.ConnectionString))
                throw new InvalidOperationException("Требуется строка подключения");

            return _options;
        }
    }
}
