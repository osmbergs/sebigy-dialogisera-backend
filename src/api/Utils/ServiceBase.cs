using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Utils;

public abstract class ServiceBase(AppDbContext db, ISessionContextAccessor sessionContextAccessor)
{
    protected AppDbContext Db { get; } = db;
    
    protected SessionContext Session => sessionContextAccessor.RequiredSession;
    
    protected SessionContext? SessionOrNull => sessionContextAccessor.Session;
    

}