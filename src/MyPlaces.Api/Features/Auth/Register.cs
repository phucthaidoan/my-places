using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public static class Register
{
    public static async Task<IResult> Handle(
        RegisterRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        if (await userManager.FindByEmailAsync(req.Email) is not null)
            return Results.BadRequest(Result<AuthResponse>.Failure("Email already in use"));

        if (await userManager.FindByNameAsync(req.Username) is not null)
            return Results.BadRequest(Result<AuthResponse>.Failure("Username already taken"));

        var user = new AppUser
        {
            Email = req.Email,
            UserName = req.Email,
            Username = req.Username,
            DisplayName = req.DisplayName
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(Result<AuthResponse>.Failure(errors));
        }

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
