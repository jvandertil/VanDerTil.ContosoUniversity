using System.Data.Common;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public interface IDatabaseSession
{
    DbConnection Connection { get; }

    DbTransaction Transaction { get; }
}
