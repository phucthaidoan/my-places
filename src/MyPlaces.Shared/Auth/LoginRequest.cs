using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
