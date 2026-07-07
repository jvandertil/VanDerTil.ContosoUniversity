using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public sealed class DefaultQueryExecutor : IQueryExecutor
{
    private readonly IDatabaseSessionManager _databaseSessionManager;
    private readonly IServiceProvider _serviceProvider;

    public DefaultQueryExecutor(IDatabaseSessionManager databaseSessionManager, IServiceProvider serviceProvider)
    {
        Guard.NotNull(databaseSessionManager);
        Guard.NotNull(serviceProvider);

        _databaseSessionManager = databaseSessionManager;
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(query);

        var session = _databaseSessionManager.CurrentSession
            ?? throw new InvalidOperationException("No active database session.");

        var context = new QueryContext(_serviceProvider);
        return query.ExecuteAsync(session, context);
    }
}
