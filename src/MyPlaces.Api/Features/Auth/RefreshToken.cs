using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public record RefreshTokenRequest(string UserId, string RefreshToken);

public static class RefreshToken
{
    public static async Task<IResult> Handle(
        RefreshTokenRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        var user = await userManager.FindByIdAsync(req.UserId);
        if (user is null)
            return Results.Unauthorized();

        var stored = await userManager.GetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken");
        if (stored != req.RefreshToken)
            return Results.Unauthorized();

        var token = tokenService.GenerateToken(user);
        var newRefresh = tokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken", newRefresh);

        return Results.Ok(Result<AuthResponse>.Success(new AuthResponse(
            Token: token,
            RefreshToken: newRefresh,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id.ToString(),
            Email: user.Email!,
            Username: user.Username,
            DisplayName: user.DisplayName
        )));
    }
}
