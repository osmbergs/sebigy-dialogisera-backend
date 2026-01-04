using System.Security.Claims;
using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Utils;

public class SessionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;

            if (userId is not null && email is not null)
            {
                var session = new SessionContext
                {
                    UserId = userId,
                    Email = email,
                    TenantId = Guid.TryParse(tenantIdClaim, out var tid) ? tid : null
                };

                SessionContextAccessor.Set(context, session);
            }
        }

        await next(context);
    }
}