using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Utils;

public class SessionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            var tenantId = context.User.FindFirst("tenant_id")?.Value;
            var userTypeString = context.User.FindFirst("user_type")?.Value;

            // Validate all required claims are present and parseable
            if (Ulid.TryParse(userId, out var parsedUserId) &&
                Ulid.TryParse(tenantId, out var parsedTenantId) &&
                Enum.TryParse<UserType>(userTypeString, out var parsedUserType))
            {
                var session = new SessionContext
                {
                    UserId = parsedUserId,
                    TenantId = parsedTenantId,
                    UserType = parsedUserType
                };

                SessionContextAccessor.Set(context, session);
            }
        }

        await next(context);
    }
}