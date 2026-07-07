namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public interface IQuery<TResult>
{
    Task<TResult> ExecuteAsync(IDatabaseSession session, QueryContext context);
}
