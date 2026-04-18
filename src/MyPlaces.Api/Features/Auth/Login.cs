using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public static class Login
{
    public static async Task<IResult> Handle(
        LoginRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
            return Results.Unauthorized();

        var token = tokenService.GenerateToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken", refreshToken);

        return Results.Ok(Result<AuthResponse>.Success(new AuthResponse(
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id.ToString(),
            Email: user.Email!,
            Username: user.Username,
            DisplayName: user.DisplayName
        )));
    }
}
