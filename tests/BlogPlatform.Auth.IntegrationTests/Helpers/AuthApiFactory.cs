using BlogPlatform.Auth.API;
using BlogPlatform.Auth.Core.Constants;
using BlogPlatform.Tests.Common;
using BlogPlatform.Tests.Common.Extensions;
using BlogPlatform.Tests.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace BlogPlatform.Auth.IntegrationTests.Helpers
{
    public class AuthApiFactory : BaseApiFactory<Program>
    {
        private const string POSTGRE_CONTAINER_NAME = "postres";
        private const string REDIS_CONTAINER_NAME = "redis";
        private const string REDIS_INSTANCE_NAME = "AuthTest";
        private const string INIT_SCRIPT_NAME = "auth-init.sql";

        private Respawner _respawner = null!;

        public AuthApiFactory()
            : base()
        {
            Containers.Add(POSTGRE_CONTAINER_NAME, new PostgreSqlBuilder()
                .WithImage("postgres:15")
                .WithDatabase("auth_test")
                .WithUsername("test_user")
                .WithPassword("test_password")
                .Build());

            Containers.Add(REDIS_CONTAINER_NAME, new RedisBuilder()
                .WithImage("redis:7-alpine")
                .Build());
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddEnvironmentVariables();
            });

            builder.ConfigureDb(
                (PostgreSqlContainer)Containers[POSTGRE_CONTAINER_NAME]);
            builder.ConfigureCache(
                (RedisContainer)Containers[REDIS_CONTAINER_NAME], REDIS_INSTANCE_NAME);

            builder.UseEnvironment("Testing");
        }

        public override async Task InitializeAsync()
        {
            await Containers[POSTGRE_CONTAINER_NAME].StartAsync();
            await Containers[REDIS_CONTAINER_NAME].StartAsync();

            using var scope = Services.CreateScope();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();

            var postgresContainer = (PostgreSqlContainer)Containers[POSTGRE_CONTAINER_NAME];
            var connectionString = postgresContainer.GetConnectionString();

            await dbInitializer.ExecuteInitScript(connectionString, INIT_SCRIPT_NAME);

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
        }

        public override async Task DisposeAsync()
        {
            await ResetAsync();

            await Containers[POSTGRE_CONTAINER_NAME].DisposeAsync();
            await Containers[REDIS_CONTAINER_NAME].DisposeAsync();
        }

        public async Task ResetAsync()
        {
            using var scope = Services.CreateScope();
            var services = scope.ServiceProvider;

            // Reset database using Respawn
            var postgresContainer = (PostgreSqlContainer)Containers[POSTGRE_CONTAINER_NAME];
            var connectionString = postgresContainer.GetConnectionString();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);

            // Reset cache
            var cache = services.GetRequiredService<IDistributedCache>();
            List<string> patterns =
            [
                $"{CacheKeys.USER_BY_ID}:*",
                $"{CacheKeys.USER_BY_EMAIL}:*",
                $"{CacheKeys.TOKEN_USER}:*",
                $"{CacheKeys.TOKEN_VALIDATION}:*"
            ];

            foreach (var pattern in patterns)
            {
                await cache.RemoveByPatternAsync(pattern);
            }
        }

        public HttpClient CreateClientWithJwt(string token)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }
    }
}