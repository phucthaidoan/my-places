namespace MyPlaces.Shared.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string Username,
    string DisplayName
);
