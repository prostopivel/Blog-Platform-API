using BlogPlatform.ApiGateway.API.Extensions;
using BlogPlatform.ApiGateway.API.Middleware;
using BlogPlatform.ApiGateway.Core.Interfaces;
using BlogPlatform.ApiGateway.Core.Services;

namespace BlogPlatform.ApiGateway.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.ConfigureSwaggerServices();
            builder.Services.AddLogging();

            builder.Services.AddHttpClient<MicroserviceClient>();

            var authServiceUrl = builder.Configuration["Microservices:Auth"];
            var blogServiceUrl = builder.Configuration["Microservices:Blog"];
            var analyticsServiceUrl = builder.Configuration["Microservices:Analytics"];

            builder.Services.AddHttpClient("AuthService", client =>
            {
                client.BaseAddress = new Uri(authServiceUrl ?? "http://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddHttpClient("BlogService", client =>
            {
                client.BaseAddress = new Uri(blogServiceUrl ?? "http://localhost:5002");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddHttpClient("AnalyticsService", client =>
            {
                client.BaseAddress = new Uri(analyticsServiceUrl ?? "http://localhost:5003");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddScoped<IMicroserviceClient, MicroserviceClient>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            //app.UseMiddleware<JwtAuthMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
