using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Middleware;

public class DatabaseSessionMiddleware
{
    private readonly RequestDelegate _next;

    public DatabaseSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var attribute = context.GetEndpoint()?.Metadata.GetMetadata<DatabaseSessionAttribute>();
        return attribute is null
            ? _next(context)
            : InvokeWithDatabaseSessionAsync(attribute, context);
    }

    private async Task InvokeWithDatabaseSessionAsync(DatabaseSessionAttribute attribute, HttpContext context)
    {
        var database = context.RequestServices.GetRequiredService<IDatabaseSessionManager>();

        var isReadOperation = HttpMethods.IsGet(context.Request.Method)
                              || HttpMethods.IsHead(context.Request.Method);

        if (isReadOperation)
        {
            await database.StartReadOnlySessionAsync(attribute.ReadIsolationLevel, context.RequestAborted);
        }
        else
        {
            await database.StartSessionAsync(attribute.WriteIsolationLevel, context.RequestAborted);
        }

        try
        {
            await _next(context);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 400)
            {
                await database.CommitSessionAsync(context.RequestAborted);
            }
            else
            {
                await database.RollbackSessionAsync(CancellationToken.None);
            }
        }
        catch
        {
            await database.RollbackSessionAsync(CancellationToken.None);

            throw;
        }
        finally
        {
            await database.EndSessionAsync();
        }
    }
}

internal static class PostgreSqlDatabaseExtensions
{
    public static async Task StartReadOnlySessionAsync(this IDatabaseSessionManager database, IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        await database.StartSessionAsync(isolationLevel, cancellationToken);
        var session = database.CurrentSession;

        if (session?.Connection is not NpgsqlConnection)
        {
            throw new InvalidOperationException("This extension method is only supported for Npgsql connections.");
        }

        await using var cmd = session.Connection.CreateCommand();
        cmd.Transaction = session.Transaction;
        cmd.CommandText = "SET TRANSACTION READ ONLY";
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
