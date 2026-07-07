using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Migrations;

public sealed class DatabaseMigrationService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly SeedingConfiguration _seedingConfiguration;
    private readonly ResiliencePipeline _retryPipeline;

    public DatabaseMigrationService(NpgsqlDataSource dataSource, SeedingConfiguration seedingConfiguration, ILogger<DatabaseMigrationService> logger)
    {
        Guard.NotNull(dataSource);
        Guard.NotNull(seedingConfiguration);
        Guard.NotNull(logger);

        _dataSource = dataSource;
        _seedingConfiguration = seedingConfiguration;
        _logger = logger;

        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromSeconds(1),
                OnRetry = _ =>
                {
                    logger.LogMigrationRetry();
                    return ValueTask.CompletedTask;
                },
                UseJitter = true,
                ShouldHandle = args => args.Outcome switch
                {
                    // Always try to retry transient errors.
                    { Exception: NpgsqlException { IsTransient: true } } => PredicateResult.True(),
                    { Exception: PostgresException { IsTransient: true } } => PredicateResult.True(),

                    // Retry if the database does not exist (yet).
                    { Exception: PostgresException { SqlState: PostgresErrorCodes.InvalidCatalogName } } => PredicateResult.True(),
                    _ => PredicateResult.False(),
                },
            })
            .Build();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);

        var sw = Stopwatch.StartNew();
        _logger.LogMigrationStarted();

        await _retryPipeline.ExecuteAsync(async ct =>
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            // Try to establish a connection to the database to ensure it's up and running
            using (var connection = await _dataSource.OpenConnectionAsync(ct)) { }

            var migrator = new DbUpDatabaseMigrationService(_dataSource, _logger);

            await migrator.MigrateDatabaseAsync();

            if (_seedingConfiguration.Enabled)
            {
                await SeedAsync(ct);
            }
        }, cancellationToken);

        _logger.LogMigrationCompleted(sw.ElapsedMilliseconds);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _logger.LogSeedingDatabase();

        var assembly = typeof(DatabaseMigrationService).Assembly;
        var seedScripts = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("SeedData", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".sql"))
            .OrderBy(name => name)
            .ToList();

        if (seedScripts.Count == 0)
        {
            return;
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        foreach (var scriptName in seedScripts)
        {
            await using var stream = assembly.GetManifestResourceStream(scriptName);
            if (stream == null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            var scriptContent = await reader.ReadToEndAsync(cancellationToken);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = scriptContent;

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
