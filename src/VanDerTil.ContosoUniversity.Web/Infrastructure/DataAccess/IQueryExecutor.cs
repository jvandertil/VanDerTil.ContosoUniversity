namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public interface IQueryExecutor
{
    Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
