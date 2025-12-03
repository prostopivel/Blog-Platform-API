namespace BlogPlatform.ApiGateway.API.DTOs
{
    public record RegisterRequest(
        string Username,
        string Email,
        string Password);
}
