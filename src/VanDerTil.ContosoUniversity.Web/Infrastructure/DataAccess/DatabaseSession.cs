using System.Data.Common;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public sealed class DatabaseSession : IDatabaseSession
{
    public DbConnection Connection { get; }

    public DbTransaction Transaction { get; }

    internal DatabaseSession(DbConnection connection, DbTransaction transaction)
    {
        Guard.NotNull(connection);
        Guard.NotNull(transaction);

        Connection = connection;
        Transaction = transaction;
    }
}
