using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public static class GoogleLogin
{
    public static async Task<IResult> Handle(
        GoogleLoginRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService,
        IConfiguration config)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [config["Google:ClientId"]]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
        }
        catch
        {
            return Results.BadRequest(Result<AuthResponse>.Failure("Invalid Google token"));
        }

        var user = await userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            var username = payload.Email.Split('@')[0].ToLower().Replace(".", "_");
            user = new AppUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                Username = username,
                DisplayName = payload.Name ?? username,
                AvatarUrl = payload.Picture,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Results.BadRequest(Result<AuthResponse>.Failure(errors));
            }
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
