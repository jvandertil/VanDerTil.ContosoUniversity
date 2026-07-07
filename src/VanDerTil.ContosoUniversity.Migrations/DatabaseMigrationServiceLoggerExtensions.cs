using Microsoft.Extensions.Logging;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Migrations;

internal static partial class DatabaseMigrationServiceLoggerExtensions
{
    [LoggerMessage(EventId = LogEvents.MigrationStarted, Level = LogLevel.Information, Message = "Starting database migration")]
    public static partial void LogMigrationStarted(this ILogger<DatabaseMigrationService> logger);

    [LoggerMessage(EventId = LogEvents.MigrationRetry, Level = LogLevel.Warning, Message = "Retrying database migration")]
    public static partial void LogMigrationRetry(this ILogger<DatabaseMigrationService> logger);

    [LoggerMessage(EventId = LogEvents.MigrationCompleted, Level = LogLevel.Information, Message = "Database migration completed after {ElapsedMilliseconds}ms")]
    public static partial void LogMigrationCompleted(this ILogger<DatabaseMigrationService> logger, long ElapsedMilliseconds);

    [LoggerMessage(EventId = LogEvents.MigrationSeedingDatabase, Level = LogLevel.Information, Message = "Seeding database")]
    public static partial void LogSeedingDatabase(this ILogger<DatabaseMigrationService> logger);
}
