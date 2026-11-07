namespace SocialApp.Application.Auth;

public record RegisterRequest(string UserName, string Email, string Password, string? DisplayName);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, Guid UserId, string UserName, string DisplayName);

public interface ITokenService
{
    string CreateToken(Guid userId, string userName, string email);
}