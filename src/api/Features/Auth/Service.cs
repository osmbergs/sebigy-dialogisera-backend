using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Sebigy.Dialogisera.Api.Features.Auth;

public class AuthService(IConfiguration config)
{
    private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(7);

    public TokenResponse? Authenticate(LoginRequest request)
    {
        // TODO: Validate credentials against your user store
        if (request.Email != "admin@example.com" || request.Password != "password")
            return null;

        return GenerateTokens(request.Email);
    }

    public TokenResponse? Refresh(RefreshRequest request)
    {
        var principal = ValidateRefreshToken(request.RefreshToken);
        if (principal is null)
            return null;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
            return null;

        return GenerateTokens(email);
    }

    private TokenResponse GenerateTokens(string email)
    {
        var accessTokenExpiry = DateTime.UtcNow.Add(_accessTokenLifetime);
        var refreshTokenExpiry = DateTime.UtcNow.Add(_refreshTokenLifetime);

        var accessToken = GenerateToken(email, accessTokenExpiry, "access");
        var refreshToken = GenerateToken(email, refreshTokenExpiry, "refresh");

        return new TokenResponse(
            accessToken,
            refreshToken,
            accessTokenExpiry,
            refreshTokenExpiry
        );
    }

    private string GenerateToken(string email, DateTime expiresAt, string tokenType)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("token_type", tokenType)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateRefreshToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out var validatedToken);

            // Ensure it's a refresh token, not an access token
            var tokenTypeClaim = principal.FindFirst("token_type")?.Value;
            if (tokenTypeClaim != "refresh")
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}