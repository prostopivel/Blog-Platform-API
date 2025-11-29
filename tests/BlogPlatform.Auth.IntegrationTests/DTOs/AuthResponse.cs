namespace BlogPlatform.Auth.IntegrationTests.DTOs
{
    public record AuthResponse(Guid UserId,
        string Username,
        string Token);
}
