using BlogPlatform.Analytics.API.HealthChecks;
using BlogPlatform.Analytics.API.Middleware;
using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Services;
using BlogPlatform.Analytics.Infrastructure.Services;
using BlogPlatform.Blog.Grpc;

namespace BlogPlatform.Analytics.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging();

            builder.Services.AddGrpcClient<BlogService.BlogServiceClient>(options =>
            {
                options.Address = new Uri(
                    builder.Configuration["BlogService:GrpcAddress"]
                    ?? "http://0.0.0.0:6002");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (builder.Environment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                return handler;
            });

            builder.Services.AddScoped<IIntervalService, IntervalService>();
            builder.Services.AddScoped<IBlogGrpcService, BlogGrpcService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

            builder.Services.AddHealthChecks()
                .AddCheck<AnalyticsServiceHealthCheck>("analytics-service");

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
