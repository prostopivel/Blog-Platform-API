using BlogPlatform.Shared.Caching.Extensions;
using BlogPlatform.Shared.Common.Models;
using BlogPlatform.Shared.Data.Extensions;
using BlogPlatform.Tests.Common.Interfaces;
using BlogPlatform.Tests.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace BlogPlatform.Tests.Common.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        private const int DEFAULT_COMMAND_TIMOUT = 10;
        private const int DEFAULT_CONNECTION_TIMOUT = 10;
        private const int DEFAULT_CACHE_TIMEOUT_MINUTES = 10;

        public static void ConfigureDb(this IWebHostBuilder builder,
            PostgreSqlContainer postgresContainer)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContext settings
                var dbSettingsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IConfigureOptions<DatabaseSettings>));
                if (dbSettingsDescriptor != null)
                {
                    services.Remove(dbSettingsDescriptor);
                }

                // Remove existing Db
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbConnection));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext with test database
                services.AddPostgresDatabase(options =>
                {
                    options.ConnectionString = postgresContainer.GetConnectionString();
                    options.CommandTimeout = DEFAULT_COMMAND_TIMOUT;
                    options.ConnectionTimeout = DEFAULT_CONNECTION_TIMOUT;
                });

                services.AddScoped<IDbInitializer, PostgresDbInitializer>();
            });
        }

        public static void ConfigureCache(this IWebHostBuilder builder,
            RedisContainer redisContainer,
            string cacheName)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing Redis settings
                var redisSettingsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IConfigureOptions<RedisSettings>));
                if (redisSettingsDescriptor != null)
                {
                    services.Remove(redisSettingsDescriptor);
                }

                // Remove existing Redis cache
                var redisDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDistributedCache));
                if (redisDescriptor != null)
                {
                    services.Remove(redisDescriptor);
                }

                // Add Redis cache with test container
                services.AddRedisCaching(options =>
                {
                    options.ConnectionString = redisContainer.GetConnectionString();
                    options.InstanceName = cacheName;
                    options.CacheTimeoutMinutes = DEFAULT_CACHE_TIMEOUT_MINUTES;
                });
            });
        }
    }
}
