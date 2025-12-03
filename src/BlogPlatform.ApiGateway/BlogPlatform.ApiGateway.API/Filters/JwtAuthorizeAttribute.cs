using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace BlogPlatform.ApiGateway.API.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class JwtAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string VALIDATION_URI = "api/auth/validate";
        private const string USER_ID_URI = "api/auth/get-userId";
        private const int HTTP_TIMEOUT_SECONDS = 10;

        private record ValidateTokenResponse(bool IsValid);
        private record UserIdResponse(Guid UserId);

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<JwtAuthorizeAttribute>>();
            var httpClientFactory = context.HttpContext.RequestServices
                .GetRequiredService<IHttpClientFactory>();

            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                SetUnauthorizedResult(context);
                return;
            }

            var jwtToken = authHeader["Bearer ".Length..];

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                context.HttpContext.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));

            var isValid = await ValidateTokenWithAuthServiceAsync(
                jwtToken,
                httpClientFactory,
                logger,
                cts.Token);

            if (!isValid)
            {
                SetUnauthorizedResult(context);
                return;
            }

            var userId = await GetUserIdFromAuthServiceAsync(
                jwtToken,
                httpClientFactory,
                logger,
                cts.Token);

            if (userId == Guid.Empty)
            {
                SetUnauthorizedResult(context);
                return;
            }

            context.HttpContext.Request.Headers["UserId"] = userId.ToString();
        }

        private async Task<bool> ValidateTokenWithAuthServiceAsync(
            string jwtToken,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = httpClientFactory.CreateClient("AuthService");
                client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);

                var request = new { token = jwtToken };
                var response = await client.PostAsJsonAsync(
                    $"{client.BaseAddress}{VALIDATION_URI}",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ValidateTokenResponse>(
                    cancellationToken: cancellationToken);

                return result?.IsValid ?? false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        private async Task<Guid> GetUserIdFromAuthServiceAsync(
            string jwtToken,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = httpClientFactory.CreateClient("AuthService");
                client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);

                var request = new { token = jwtToken };
                var response = await client.PostAsJsonAsync(
                    $"{client.BaseAddress}{USER_ID_URI}",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<UserIdResponse>(
                    cancellationToken: cancellationToken);

                return result?.UserId ?? Guid.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting user id");
                return Guid.Empty;
            }
        }

        private static void SetUnauthorizedResult(AuthorizationFilterContext context)
        {
            context.Result = new ObjectResult(new
            {
                Message = "Unauthorized",
                StatusCode = (int)HttpStatusCode.Unauthorized
            })
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
        }
    }
}
