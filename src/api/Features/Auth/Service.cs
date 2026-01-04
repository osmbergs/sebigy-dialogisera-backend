using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Utils;

namespace Sebigy.Dialogisera.Api.Features.Auth;

public class AuthService(IConfiguration config,AppDbContext db)
{
    private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(7);

    async public Task<TokenResponse?> Authenticate(LoginRequest request)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user is null)
            return null;

        if (!CryptHelper.IsHashEqual(request.Password, user.PasswordHash))
            return null;
        
        return GenerateTokens(user);
    }

    public async Task<TokenResponse?> Refresh(RefreshRequest request)
    {
        var principal = ValidateRefreshToken(request.RefreshToken);
        if (principal is null)
            return null;
        
        if (!Ulid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ulidId))
            return null;
      
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == ulidId);


        return GenerateTokens(user);
    }

    private TokenResponse GenerateTokens(User u)
    {
        var accessTokenExpiry = DateTime.UtcNow.Add(_accessTokenLifetime);
        var refreshTokenExpiry = DateTime.UtcNow.Add(_refreshTokenLifetime);

        var accessToken = GenerateToken(u, accessTokenExpiry, "access");
        var refreshToken = GenerateToken(u, refreshTokenExpiry, "refresh");

        return new TokenResponse(
            accessToken,
            refreshToken,
            accessTokenExpiry,
            refreshTokenExpiry
        );
    }

    private string GenerateToken(User u, DateTime expiresAt, string tokenType)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
       //     new Claim(ClaimTypes.Email, u.Email),
        //    new Claim(ClaimTypes.NameIdentifier,u.Id.ToString()),
            new Claim("token_type", tokenType),
            new Claim("user_type",u.Type.ToString()),
            new Claim("user_id",u.Id.ToString()), 
            new Claim("tenant_id",u.TenantId.ToString())
            
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