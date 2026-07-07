using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public sealed class DbDataSourceDatabaseSessionManager : IDatabaseSessionManager
{
    private readonly DbDataSource _dataSource;

    public IDatabaseSession? CurrentSession { get; private set; }

    public DbDataSourceDatabaseSessionManager(DbDataSource dataSource)
    {
        Guard.NotNull(dataSource);

        _dataSource = dataSource;
    }

    public Task StartSessionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (CurrentSession is not null)
        {
            throw new InvalidOperationException("A database session is already active.");
        }

        return StartSessionInternalAsync();

        async Task StartSessionInternalAsync()
        {
            var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
                CurrentSession = new DatabaseSession(connection, transaction);
            }
            catch
            {
                await connection.SafeDisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
    }

    public Task CommitSessionAsync(CancellationToken cancellationToken = default)
    {
        EnsureSessionActive();

        return CommitSessionInternalAsync();

        async Task CommitSessionInternalAsync()
        {
            try
            {
                await CurrentSession.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await EndSessionAsync().ConfigureAwait(false);
            }
        }
    }

    public Task RollbackSessionAsync(CancellationToken cancellationToken = default)
    {
        EnsureSessionActive();

        return RollbackSessionInternalAsync();

        async Task RollbackSessionInternalAsync()
        {
            try
            {
                await CurrentSession.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await EndSessionAsync().ConfigureAwait(false);
            }
        }
    }

    public async ValueTask EndSessionAsync()
    {
        if (CurrentSession is not null)
        {
            await CurrentSession.Transaction.SafeDisposeAsync().ConfigureAwait(false);
            await CurrentSession.Connection.SafeDisposeAsync().ConfigureAwait(false);

            CurrentSession = null;
        }
    }

    [MemberNotNull(nameof(CurrentSession))]
    private void EnsureSessionActive()
    {
        if (CurrentSession is null)
        {
            throw new InvalidOperationException("No active database session.");
        }
    }
}
