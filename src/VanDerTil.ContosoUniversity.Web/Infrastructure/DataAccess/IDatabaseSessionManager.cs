using System.Data;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

/// <summary>
/// Defines the contract for managing database sessions, including starting, committing, rolling back, and ending
/// transactional sessions.
/// </summary>
/// <remarks>Implementations of this interface provide session-based control over database transactions, allowing
/// callers to explicitly manage transaction boundaries and isolation levels. Methods are asynchronous to support
/// non-blocking database operations. Callers should ensure proper session lifecycle management to avoid resource leaks
/// or inconsistent state.</remarks>
public interface IDatabaseSessionManager
{
    /// <summary>
    /// Gets the current database session associated with the context, if any.
    /// </summary>
    IDatabaseSession? CurrentSession { get; }

    /// <summary>
    /// Begins a new session with the specified transaction isolation level.
    /// </summary>
    /// <param name="isolationLevel">The transaction isolation level to use for the session. Determines how data modifications are visible to other
    /// operations during the session.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a session is already active.</exception>"
    Task StartSessionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits all pending changes in the current session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the commit operation.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is no active session to commit.</exception>"
    Task CommitSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously rolls back the current session, discarding any uncommitted changes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the rollback operation.</param>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is no active session to rollback.</exception>
    Task RollbackSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the current session asynchronously.
    /// </summary>
    /// <remarks>Does not throw when no session is active.</remarks>
    /// <returns>A ValueTask that represents the asynchronous operation.</returns>
    ValueTask EndSessionAsync();
}
