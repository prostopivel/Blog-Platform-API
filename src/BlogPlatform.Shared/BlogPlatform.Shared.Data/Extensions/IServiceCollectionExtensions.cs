using BlogPlatform.Shared.Common.Models;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace BlogPlatform.Shared.Data.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgresDatabase(
            this IServiceCollection services,
             Action<DatabaseSettings> configureOptions)
        {
            var settings = new DatabaseSettings();
            configureOptions(settings);

            services.Configure<DatabaseSettings>(opt =>
            {
                opt.ConnectionString = settings.ConnectionString;
                opt.CommandTimeout = settings.CommandTimeout;
                opt.ConnectionTimeout = settings.ConnectionTimeout;
            });

            services.AddScoped<IDbConnection>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(options.ConnectionString)
                {
                    CommandTimeout = options.CommandTimeout,
                    Timeout = options.ConnectionTimeout
                };

                var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                return connection;
            });

            ConfigureDapper();

            return services;
        }

        private static void ConfigureDapper()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
        }
    }
}
