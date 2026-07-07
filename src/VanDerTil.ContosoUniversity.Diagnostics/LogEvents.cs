namespace VanDerTil.ContosoUniversity.Diagnostics;

/// <summary>
/// Event IDs for structured logging across the application.
/// IDs follow the scheme XYZZZ where X=area, Y=category, ZZZ=event number (000-999).
/// </summary>
public static class LogEvents
{
    // 11000 - 11999: Infrastructure

    // 11100 - 11110: Database Migrations
    public const int MigrationStarted = 11100;
    public const int MigrationRetry = 11101;
    public const int MigrationCompleted = 11102;
    public const int MigrationCancelled = 11103;
    public const int MigrationFailed = 11104;
    public const int MigrationSeedingDatabase = 11105;
}
