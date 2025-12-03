using BlogPlatform.ApiGateway.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace BlogPlatform.ApiGateway.Core.Services
{
    public class MicroserviceClient : IMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public MicroserviceClient(HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<HttpResponseMessage> ForwardRequestAsync(
            HttpRequest request,
            string serviceName,
            string path,
            CancellationToken token = default)
        {
            var requestMessage = CreateRequestMessage(request, serviceName, path);
            SetRequestBodyAsync(request, requestMessage);

            return await _httpClient.SendAsync(requestMessage, token);
        }

        public async Task<HttpResponseMessage> ForwardRequestAsync<T>(
            HttpRequest request,
            string serviceName,
            string path,
            T? body = null,
            CancellationToken token = default)
            where T : class
        {
            var requestMessage = CreateRequestMessage(request, serviceName, path);
            var method = requestMessage.Method;

            if (body != null
                && (method == HttpMethod.Post
                || method == HttpMethod.Put
                || method == HttpMethod.Patch))
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                requestMessage.Content = new StringContent(
                    json, Encoding.UTF8, "application/json");
            }
            else
            {
                SetRequestBodyAsync(request, requestMessage);
            }

            return await _httpClient.SendAsync(requestMessage, token);
        }

        private HttpRequestMessage CreateRequestMessage(HttpRequest request,
            string serviceName,
            string path)
        {
            var serviceUrl = _configuration[$"Microservices:{serviceName}"];
            var targetUrl = $"{serviceUrl}/{path.TrimStart('/')}";

            var method = new HttpMethod(request.Method);
            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(targetUrl),
                Method = method
            };

            foreach (var header in request.Headers)
            {
                if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                    && !header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                    && !header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]);
                }
            }

            return requestMessage;
        }

        private static void SetRequestBodyAsync(HttpRequest request,
            HttpRequestMessage requestMessage)
        {
            var method = requestMessage.Method;

            if ((method == HttpMethod.Post
                || method == HttpMethod.Put
                || method == HttpMethod.Patch)
                && (request.ContentLength > 0 || request.Body?.Length > 0))
            {
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }
                requestMessage.Content = new StreamContent(request.Body);

                if (request.ContentType != null)
                {
                    requestMessage.Content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
                }

                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }
            }
        }
    }
}