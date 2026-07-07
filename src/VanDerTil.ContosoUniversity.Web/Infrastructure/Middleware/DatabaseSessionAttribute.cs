using System.Data;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Middleware;

/// <summary>
/// Marks a class as requiring an ambient database session.
/// The database session middleware starts a transaction before the handler runs,
/// selecting the isolation level based on the HTTP method:
/// read operations (GET, HEAD) use <see cref="ReadIsolationLevel"/>,
/// write operations (POST, PUT, PATCH, DELETE) use <see cref="WriteIsolationLevel"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DatabaseSessionAttribute : Attribute
{
    /// <summary>
    /// Isolation level used for read operations (GET, HEAD).
    /// Defaults to <see cref="IsolationLevel.ReadCommitted"/>.
    /// </summary>
    public IsolationLevel ReadIsolationLevel { get; }

    /// <summary>
    /// Isolation level used for write operations (POST, PUT, PATCH, DELETE).
    /// Defaults to <see cref="IsolationLevel.RepeatableRead"/>.
    /// </summary>
    public IsolationLevel WriteIsolationLevel { get; }

    public DatabaseSessionAttribute(
        IsolationLevel readIsolationLevel = IsolationLevel.ReadCommitted,
        IsolationLevel writeIsolationLevel = IsolationLevel.RepeatableRead)
    {
        ReadIsolationLevel = readIsolationLevel;
        WriteIsolationLevel = writeIsolationLevel;
    }
}
