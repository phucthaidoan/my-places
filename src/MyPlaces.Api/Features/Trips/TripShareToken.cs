using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Features.Trips;

internal static class TripShareToken
{
    private const int MaxAttempts = 8;

    /// <summary>URL-safe token (~43 chars from 32 random bytes).</summary>
    public static string Generate()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static async Task<string> GenerateUniqueAsync(AppDbContext db, CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var token = Generate();
            var exists = await db.Trips.AnyAsync(t => t.ShareToken == token, ct);
            if (!exists)
                return token;
        }

        throw new InvalidOperationException("Could not generate a unique share token.");
    }
}
