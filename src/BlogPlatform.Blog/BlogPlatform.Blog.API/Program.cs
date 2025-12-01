using BlogPlatform.Blog.API.HealthChecks;
using BlogPlatform.Blog.API.Mapping;
using BlogPlatform.Blog.API.Middleware;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Services;
using BlogPlatform.Blog.Infrastructure;
using BlogPlatform.Blog.Infrastructure.Mapping;
using BlogPlatform.Blog.Infrastructure.Repositories;
using BlogPlatform.Shared.Caching.Extensions;
using BlogPlatform.Shared.Caching.Interfaces;
using BlogPlatform.Shared.Caching.Services;
using BlogPlatform.Shared.Common.Interfaces;
using BlogPlatform.Shared.Common.Models;
using BlogPlatform.Shared.Data.Extensions;
using BlogPlatform.Shared.Data.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Blog.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            SqlMapperConfigurator.ConfigureEntities();

            var redisSettings = builder.Configuration.GetSection("Redis")
                .Get<RedisSettings>()!;
            var databaseSettings = builder.Configuration.GetSection("Database")
                .Get<DatabaseSettings>()!;

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging();
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<BlogMappingProfile>();
                cfg.AddProfile<BlogApiMappingProfile>();
            });

            builder.Services.AddPostgresDatabase(options =>
            {
                options.ConnectionString = databaseSettings.ConnectionString;
                options.CommandTimeout = databaseSettings.CommandTimeout;
                options.ConnectionTimeout = databaseSettings.ConnectionTimeout;
            });

            builder.Services.AddRedisCaching(options =>
            {
                options.ConnectionString = redisSettings.ConnectionString;
                options.CacheTimeoutMinutes = redisSettings.CacheTimeoutMinutes;
                options.InstanceName = redisSettings.InstanceName;
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ILikeRepository, LikeRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<ICacheService, RedisCacheService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IPostService, PostService>();

            builder.Services.AddHealthChecks()
                .AddNpgSql(databaseSettings.ConnectionString, name: "blog-db")
                .AddRedis(redisSettings.ConnectionString, name: "redis")
                .AddCheck<BlogServiceHealthCheck>("blog-service",
                HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(10));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.MapControllers();

            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
