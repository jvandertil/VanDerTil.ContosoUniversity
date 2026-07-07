using Microsoft.Extensions.Logging;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Migrations;

internal static partial class ProgramLoggerExtensions
{
    [LoggerMessage(EventId = LogEvents.MigrationCompleted, Level = LogLevel.Information, Message = "Database migrations completed successfully.")]
    public static partial void LogMigrationsCompleted(this ILogger<Program> logger);

    [LoggerMessage(EventId = LogEvents.MigrationCancelled, Level = LogLevel.Warning, Message = "Database migration was cancelled.")]
    public static partial void LogMigrationCancelled(this ILogger<Program> logger);

    [LoggerMessage(EventId = LogEvents.MigrationFailed, Level = LogLevel.Error, Message = "Database migration failed.")]
    public static partial void LogMigrationFailed(this ILogger<Program> logger, Exception ex);
}
