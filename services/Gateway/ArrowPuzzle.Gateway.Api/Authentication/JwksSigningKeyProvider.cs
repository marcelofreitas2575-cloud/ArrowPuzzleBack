using Microsoft.IdentityModel.Tokens;

namespace ArrowPuzzle.Gateway.Api.Authentication;

internal static class JwksSigningKeyProvider
{
    private static readonly HttpClient HttpClient = new();
    private static readonly Lock Sync = new();
    private static string? _cachedUri;
    private static IReadOnlyCollection<SecurityKey> _cachedKeys = [];
    private static DateTime _cachedAt = DateTime.MinValue;

    public static IEnumerable<SecurityKey> Resolve(string jwksUri, string? keyId)
    {
        var keys = GetSigningKeys(jwksUri);
        if (string.IsNullOrEmpty(keyId))
        {
            return keys;
        }

        return keys.Where(key => key.KeyId == keyId);
    }

    private static IReadOnlyCollection<SecurityKey> GetSigningKeys(string jwksUri)
    {
        lock (Sync)
        {
            if (_cachedUri == jwksUri
                && _cachedKeys.Count > 0
                && DateTime.UtcNow - _cachedAt < TimeSpan.FromMinutes(10))
            {
                return _cachedKeys;
            }

            var json = HttpClient.GetStringAsync(jwksUri).GetAwaiter().GetResult();
            var keySet = new JsonWebKeySet(json);
            _cachedUri = jwksUri;
            _cachedKeys = keySet.GetSigningKeys().ToList();
            _cachedAt = DateTime.UtcNow;
            return _cachedKeys;
        }
    }
}
