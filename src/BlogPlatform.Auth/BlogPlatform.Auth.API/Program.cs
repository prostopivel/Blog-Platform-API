using BlogPlatform.Auth.API.HealthChecks;
using BlogPlatform.Auth.API.Middleware;
using BlogPlatform.Auth.Core.Interfaces.Repositories;
using BlogPlatform.Auth.Core.Interfaces.Services;
using BlogPlatform.Auth.Core.Services;
using BlogPlatform.Auth.Grpc.Services;
using BlogPlatform.Auth.Infrastructure.Data;
using BlogPlatform.Auth.Infrastructure.Mapping;
using BlogPlatform.Auth.Infrastructure.Repositories;
using BlogPlatform.Shared.Caching.Interfaces;
using BlogPlatform.Shared.Caching.Services;
using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Auth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var redisSettings = builder.Configuration.GetSection("Redis")
                .Get<RedisSettings>()!;
            var databaseSettings = builder.Configuration.GetSection("Database")
                .Get<DatabaseSettings>()!;
            var jwtSettings = builder.Configuration.GetSection("Jwt")
                .Get<JwtSettings>()!;

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<RedisSettings>(
                builder.Configuration.GetSection("Redis"));
            builder.Services.Configure<DatabaseSettings>(
                builder.Configuration.GetSection("Database"));

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
            });

            builder.Services.AddPostgresDatabase(options =>
                options.WithConnectionString(databaseSettings.ConnectionString));

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString;
                options.InstanceName = redisSettings.InstanceName;
            });

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICacheService, RedisCacheService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddGrpc();

            builder.Services.AddHealthChecks()
                .AddNpgSql(databaseSettings.ConnectionString, name: "auth-db")
                .AddRedis(redisSettings.ConnectionString, name: "redis")
                .AddCheck<AuthServiceHealthCheck>("auth-service");

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
            app.MapGrpcService<AuthGrpcService>();

            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
