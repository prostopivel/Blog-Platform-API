using BlogPlatform.Shared.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Shared.Caching.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCaching(
            this IServiceCollection services,
            Action<RedisSettings> configureOptions)
        {
            var settings = new RedisSettings();
            configureOptions(settings);

            services.Configure<RedisSettings>(opt =>
            {
                opt.ConnectionString = settings.ConnectionString;
                opt.InstanceName = settings.InstanceName;
                opt.CacheTimeoutMinutes = settings.CacheTimeoutMinutes;
            });

            services.AddStackExchangeRedisCache(cacheOptions =>
            {
                cacheOptions.Configuration = settings.ConnectionString;
                cacheOptions.InstanceName = settings.InstanceName;
            });

            return services;
        }
    }
}
