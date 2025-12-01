using BlogPlatform.Blog.API;
using BlogPlatform.Blog.Core.Constants;
using BlogPlatform.Blog.Infrastructure;
using BlogPlatform.Tests.Common;
using BlogPlatform.Tests.Common.Extensions;
using BlogPlatform.Tests.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace BlogPlatform.Blog.IntegrationTests.Helpers
{
    public class BlogApiFactory : BaseApiFactory<Program>
    {
        private const string POSTGRE_CONTAINER_NAME = "blog-db";
        private const string REDIS_CONTAINER_NAME = "redis";
        private const string REDIS_INSTANCE_NAME = "BlogTest";
        private const string INIT_SCRIPT_NAME = "blog-init.sql";

        private Respawner _respawner = null!;

        public BlogApiFactory()
            : base()
        {
            Containers.Add(POSTGRE_CONTAINER_NAME, new PostgreSqlBuilder()
                .WithImage("postgres:15")
                .WithDatabase("blog_test")
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

            SqlMapperConfigurator.ConfigureEntities();
            builder.ConfigureDb(
                (PostgreSqlContainer)Containers[POSTGRE_CONTAINER_NAME]);
            builder.ConfigureCache(
                (RedisContainer)Containers[REDIS_CONTAINER_NAME], REDIS_INSTANCE_NAME);

            builder.UseEnvironment("Testing");

            base.ConfigureWebHost(builder);
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
                SchemasToInclude = ["public"],
                WithReseed = true,
                CommandTimeout = 60
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
            await Task.Delay(100);

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
                $"{CacheKeys.TAGS_BY_POST_ID}:*",
                $"{CacheKeys.POST_BY_ID}:*",
                $"{CacheKeys.TAG_BY_NAME}:*"
            ];

            foreach (var pattern in patterns)
            {
                await cache.RemoveByPatternAsync(pattern);
            }
        }
    }
}