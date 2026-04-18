using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(3)] string Username,
    [Required, MinLength(6)] string Password,
    [Required] string DisplayName
);
