using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Utils;

public interface ISessionContextAccessor
{
    SessionContext? Session { get; }
    SessionContext RequiredSession { get; }
}

public class SessionContextAccessor(IHttpContextAccessor httpContextAccessor) : ISessionContextAccessor
{
    private const string SessionContextKey = "SessionContext";

    public SessionContext? Session =>
        httpContextAccessor.HttpContext?.Items[SessionContextKey] as SessionContext;

    public SessionContext RequiredSession =>
        Session ?? throw new InvalidOperationException("No authenticated session available");

    public static void Set(HttpContext context, SessionContext session)
    {
        context.Items[SessionContextKey] = session;
    }
}