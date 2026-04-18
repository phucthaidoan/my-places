using System.Security.Claims;

namespace MyPlaces.Api.Common;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
}

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id => Guid.Parse(
        accessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("User not authenticated"));

    public string Email => accessor.HttpContext!.User.FindFirstValue(ClaimTypes.Email)
        ?? throw new InvalidOperationException("User not authenticated");
}
