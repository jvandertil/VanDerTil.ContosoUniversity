using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

public sealed class QueryContext
{
    public QueryContext(IServiceProvider serviceProvider)
    {
        Guard.NotNull(serviceProvider);

        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }
}
