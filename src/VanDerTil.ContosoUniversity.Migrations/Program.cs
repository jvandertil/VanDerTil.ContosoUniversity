using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VanDerTil.ContosoUniversity.Migrations;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("contosodb");

var seedingEnabled = builder.Configuration.GetValue("Seeding:Enabled", true);
builder.Services.AddSingleton<DatabaseMigrationService>();
builder.Services.AddSingleton(new SeedingConfiguration { Enabled = seedingEnabled });

using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    var migrationService = host.Services.GetRequiredService<DatabaseMigrationService>();
    await migrationService.RunAsync(cancellationTokenSource.Token);

    logger.LogMigrationsCompleted();
    return 0;
}
catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
{
    logger.LogMigrationCancelled();
    return 130;
}
catch (Exception exception)
{
    logger.LogMigrationFailed(exception);
    return 1;
}
