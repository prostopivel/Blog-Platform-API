namespace BlogPlatform.ApiGateway.API.Middleware
{
    public class JwtAuthMiddleware
    {
        private const string VALIDATION_URI = "/api/auth/validate";
        private const string USER_ID_URI = "/api/auth/get-userId";
        private const int HTTP_TIMEOUT_SECONDS = 5;

        private record ValidateTokenResponse(bool IsValid);
        private record UserIdResponse(Guid UserId);

        private readonly RequestDelegate _next;
        private readonly ILogger<JwtAuthMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HashSet<string> _publicEndpoints;

        public JwtAuthMiddleware(
            RequestDelegate next,
            ILogger<JwtAuthMiddleware> logger,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            _publicEndpoints =
            [
                "/api/auth/register",
                "/api/auth/login",
                "/api/auth/validate",
                "/api/auth/get-userId",
                "/api/posts/",
                "/api/comments/by-post/",
                "/api/analytics/tag-statisics",
                "/api/analytics/by-date-range",
                "/swagger",
                "/swagger/v1/swagger.json",
                "/swagger/index.html"
            ];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (IsPublicEndpoint(path, context.Request.Method))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                ReturnUnauthorized(context);
                return;
            }

            var jwtToken = authHeader["Bearer ".Length..];

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                context.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));

            var isValid = await ValidateTokenWithAuthServiceAsync(jwtToken, cts.Token);
            if (!isValid)
            {
                ReturnUnauthorized(context);
                return;
            }

            var userId = await GetUserIdFromAuthServiceAsync(jwtToken, cts.Token);
            if (userId == Guid.Empty)
            {
                ReturnUnauthorized(context);
                return;
            }

            context.Request.Headers["UserId"] = userId.ToString();

            await _next(context);
        }

        private bool IsPublicEndpoint(string path, string method)
        {
            if (_publicEndpoints.Any(e => path.StartsWith(e)))
            {
                return true;
            }

            if (path.StartsWith("/api/posts/") && method == HttpMethods.Get)
            {
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length >= 3)
                {
                    var action = segments[2];

                    // GET /api/posts/{id}
                    if (Guid.TryParse(action, out _))
                    {
                        return true;
                    }

                    // GET /api/posts/by-tags
                    // GET /api/posts/by-user
                    if (action == "by-tags" || action == "by-user")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> ValidateTokenWithAuthServiceAsync(
            string jwtToken,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);

                var request = new
                {
                    token = jwtToken
                };

                var response = await client.PostAsJsonAsync(
                    client.BaseAddress + VALIDATION_URI,
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ValidateTokenResponse>(
                    cancellationToken: cancellationToken);

                return result?.IsValid ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        private async Task<Guid> GetUserIdFromAuthServiceAsync(
            string jwtToken,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);

                var request = new
                {
                    token = jwtToken
                };

                var response = await client.PostAsJsonAsync(
                    client.BaseAddress + USER_ID_URI,
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<UserIdResponse>(
                    cancellationToken: cancellationToken);

                return result?.UserId ?? Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user id");
                return Guid.Empty;
            }
        }

        private static void ReturnUnauthorized(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer";
        }
    }
}