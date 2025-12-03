using BlogPlatform.ApiGateway.Core.Exceptions;
using BlogPlatform.ApiGateway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.ApiGateway.API.Controllers
{
    public abstract class GatewayControllerBase : ControllerBase
    {
        protected readonly IMicroserviceClient _microserviceClient;

        public GatewayControllerBase(IMicroserviceClient microserviceClient)
        {
            _microserviceClient = microserviceClient;
        }

        protected string GetUserId()
        {
            var userId = Request.Headers["UserId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            return userId;
        }

        protected async Task<IActionResult> ProcessResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ContentResult
                {
                    Content = content,
                    ContentType = response.Content.Headers.ContentType?.ToString(),
                    StatusCode = (int)response.StatusCode
                };
            }

            return StatusCode((int)response.StatusCode, content);
        }
    }
}
