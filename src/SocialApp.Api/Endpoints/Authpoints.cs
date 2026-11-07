using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SocialApp.Application.Auth;
using SocialApp.Infrastructure.Identity;

namespace SocialApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterRequest req,
            UserManager<ApplicationUser> userManager,
            ITokenService tokens) =>
        {
            var user = new ApplicationUser
            {
                UserName = req.UserName,
                Email = req.Email,
                DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? req.UserName : req.DisplayName
            };

            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return Results.ValidationProblem(errors);
            }

            var token = tokens.CreateToken(user.Id, user.UserName!, user.Email!);
            return Results.Ok(new AuthResponse(token, user.Id, user.UserName!, user.DisplayName));
        });

        group.MapPost("/login", async (
            LoginRequest req,
            UserManager<ApplicationUser> userManager,
            ITokenService tokens) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized(); // cố ý không nói sai email hay sai mật khẩu

            var token = tokens.CreateToken(user.Id, user.UserName!, user.Email!);
            return Results.Ok(new AuthResponse(token, user.Id, user.UserName!, user.DisplayName));
        });

        // Endpoint thử nghiệm: chỉ người có token hợp lệ mới vào được
        group.MapGet("/me", (ClaimsPrincipal principal) => Results.Ok(new
        {
            userId = principal.FindFirstValue("sub"),
            userName = principal.FindFirstValue("unique_name"),
            email = principal.FindFirstValue("email")
        })).RequireAuthorization();
    }
}