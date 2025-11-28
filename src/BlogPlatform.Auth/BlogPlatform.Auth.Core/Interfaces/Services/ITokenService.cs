using BlogPlatform.Auth.Core.Entities;

namespace BlogPlatform.Auth.Core.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        (bool isValid, Guid? userId) ValidateToken(string token);
    }
}
