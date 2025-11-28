using BlogPlatform.Auth.Core.Interfaces.Repositories;
using BlogPlatform.Auth.Infrastructure.Data.Options;
using BlogPlatform.Auth.Infrastructure.Repositories;
using BlogPlatform.Shared.Common.Models;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace BlogPlatform.Auth.Infrastructure.Data
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgresDatabase(
            this IServiceCollection services,
            Action<DbOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new DbOptionsBuilder();
            configureOptions(optionsBuilder);
            var options = optionsBuilder.Build();

            services.AddSingleton(options);
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IDbConnection>(provider =>
            {
                var options = provider.GetRequiredService<DatabaseSettings>();
                var connection = new NpgsqlConnection(options.ConnectionString);
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
