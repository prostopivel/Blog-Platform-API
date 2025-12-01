using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Xunit;

namespace BlogPlatform.Tests.Common
{
    public abstract class BaseApiFactory<T>
        : WebApplicationFactory<T>, IAsyncLifetime
        where T : class
    {
        protected Dictionary<string, IContainer> Containers { get; init; }

        protected BaseApiFactory()
        {
            Containers = [];
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<HttpClientFactoryOptions>(options =>
                {
                    options.HttpMessageHandlerBuilderActions.Add(builder =>
                    {
                        builder.PrimaryHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                    });
                });
                services.AddLogging();
            });
        }

        public HttpClient CreateClientWithUserId(string? userId = null)
        {
            var client = CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            if (userId != null)
            {
                client.DefaultRequestHeaders.Add("userId", userId);
            }
            return client;
        }

        public abstract Task InitializeAsync();
        public abstract new Task DisposeAsync();
    }
}