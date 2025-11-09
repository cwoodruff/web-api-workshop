using System.Security.Claims;

namespace Chinook.API.Services;

public interface ITokenService
{
    string CreateAccessToken(IEnumerable<Claim> claims, DateTimeOffset? expires = null);
}