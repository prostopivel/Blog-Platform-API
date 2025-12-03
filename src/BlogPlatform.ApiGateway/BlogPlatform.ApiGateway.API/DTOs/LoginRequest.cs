namespace BlogPlatform.ApiGateway.API.DTOs
{
    public record LoginRequest(
        string Email,
        string Password);
}
