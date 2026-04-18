using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Auth;

public record GoogleLoginRequest(
    [Required] string IdToken
);
