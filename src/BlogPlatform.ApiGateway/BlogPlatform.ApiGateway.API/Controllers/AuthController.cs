using BlogPlatform.ApiGateway.API.DTOs;
using BlogPlatform.ApiGateway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.ApiGateway.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : GatewayControllerBase
    {
        public AuthController(IMicroserviceClient microserviceClient)
            : base(microserviceClient)
        {
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request,
            CancellationToken token = default)
        {
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Auth", $"api/auth/register", request,
                token: token);

            return await ProcessResponse(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request,
            CancellationToken token = default)
        {
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Auth", $"api/auth/login", request,
                token: token);

            return await ProcessResponse(response);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateTokenRequest request,
            CancellationToken token = default)
        {
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Auth", $"api/auth/validate", request,
                token: token);

            return await ProcessResponse(response);
        }
    }
}
