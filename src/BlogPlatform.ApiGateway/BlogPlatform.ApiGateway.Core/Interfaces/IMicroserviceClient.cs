using Microsoft.AspNetCore.Http;

namespace BlogPlatform.ApiGateway.Core.Interfaces
{
    public interface IMicroserviceClient
    {
        Task<HttpResponseMessage> ForwardRequestAsync(HttpRequest request,
            string serviceName, string path,
            CancellationToken token = default);

        Task<HttpResponseMessage> ForwardRequestAsync<T>(HttpRequest request,
            string serviceName, string path, T? body = null,
            CancellationToken token = default) where T : class;
    }
}
