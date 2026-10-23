using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArrowPuzzle.Gateway.Api.Authentication;

internal static class UserClaims
{
    public static string GetSubjectId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(sub))
        {
            throw new InvalidOperationException("Authenticated user is missing the 'sub' claim.");
        }

        return sub;
    }
}
