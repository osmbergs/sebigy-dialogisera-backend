// Utils/ServiceBase.cs
using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Utils;

public abstract class ServiceBase
{
    protected readonly AppDbContext Db;
    private readonly ISessionContextAccessor _sessionContextAccessor;
    
    // Use RequiredSession which throws if no session is available
    protected SessionContext SessionContext => _sessionContextAccessor.RequiredSession;

    protected ServiceBase(AppDbContext db, ISessionContextAccessor sessionContextAccessor)
    {
        Db = db;
        _sessionContextAccessor = sessionContextAccessor;
    }

    /// <summary>
    /// Throws UnauthorizedAccessException if current user doesn't have required type
    /// </summary>
    protected void RequireUserType(params UserType[] allowedTypes)
    {
        if (!allowedTypes.Contains(SessionContext.UserType))
        {
            throw new UnauthorizedAccessException(
                $"This operation requires one of the following user types: {string.Join(", ", allowedTypes)}");
        }
    }

    /// <summary>
    /// Throws UnauthorizedAccessException if current user is not Root
    /// </summary>
    protected void RequireRoot()
    {
        RequireUserType(UserType.Root);
    }

    /// <summary>
    /// Throws UnauthorizedAccessException if current user is not Root or Admin
    /// </summary>
    protected void RequireAdminOrRoot()
    {
        RequireUserType(UserType.Root, UserType.Admin);
    }
}