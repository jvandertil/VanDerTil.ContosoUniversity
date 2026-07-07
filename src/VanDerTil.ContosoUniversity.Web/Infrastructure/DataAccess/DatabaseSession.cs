using System.Data.Common;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public sealed class DatabaseSession
{
    public DbConnection Connection { get; }

    public DbTransaction Transaction { get; }

    internal DatabaseSession(DbConnection connection, DbTransaction transaction)
    {
        Connection = connection;
        Transaction = transaction;
    }
}
