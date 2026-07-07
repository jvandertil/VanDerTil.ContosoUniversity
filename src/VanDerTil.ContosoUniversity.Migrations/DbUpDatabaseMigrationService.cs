using System.Data;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.Postgresql;
using Microsoft.Extensions.Logging;
using Npgsql;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Migrations;

public sealed class DbUpDatabaseMigrationService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger _logger;

    public DbUpDatabaseMigrationService(NpgsqlDataSource dataSource, ILogger logger)
    {
        Guard.NotNull(dataSource);
        Guard.NotNull(logger);

        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task MigrateDatabaseAsync()
    {
        var connectionManager = new PostgresqlConnectionManager(_dataSource);

        var upgradeEngine = DeployChanges.To
            .PostgresqlDatabase(connectionManager)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                script => !script.Contains("SeedData", StringComparison.OrdinalIgnoreCase))
            .LogTo(_logger)
            .JournalTo(new CustomSchemaVersionsJournal(connectionManager))
            .Build();

        await Task.Run(() => upgradeEngine.PerformUpgrade());
    }

    private sealed class CustomSchemaVersionsJournal : IJournal
    {
        private readonly string _schema;
        private readonly IConnectionManager _connectionManager;

        public CustomSchemaVersionsJournal(IConnectionManager connectionManager)
            : this(null, connectionManager)
        {
            Ensure.That(connectionManager is not null);
        }

        public CustomSchemaVersionsJournal(string? schema, IConnectionManager connectionManager)
        {
            Ensure.That(connectionManager is not null);

            _schema = schema ?? "public";
            _connectionManager = connectionManager;
        }

        public void EnsureTableExistsAndIsLatestVersion(Func<IDbCommand> dbCommandFactory)
        {
            using var cmd = dbCommandFactory();
            cmd.CommandText = $"""
                CREATE TABLE IF NOT EXISTS {_schema}.schema_versions (
                    id          BIGINT                   NOT NULL GENERATED ALWAYS AS IDENTITY,
                    script_name TEXT                     NOT NULL,
                    applied     TIMESTAMP WITH TIME ZONE NOT NULL,

                    CONSTRAINT pk_schema_versions PRIMARY KEY ( id )
                )
                """;

            cmd.ExecuteNonQuery();
        }

        public string[] GetExecutedScripts()
        {
            return _connectionManager.ExecuteCommandsWithManagedConnection<string[]>(cmdFactory =>
            {
                using var existsQuery = cmdFactory();
                existsQuery.CommandText = $"""
                    SELECT EXISTS (
                        SELECT FROM pg_tables
                        WHERE  schemaname = '{_schema}'
                        AND    tablename  = 'schema_versions'
                    );
                    """;

                var existsResult = (bool?)existsQuery.ExecuteScalar();

                if (existsResult != true)
                {
                    return [];
                }

                using var retrieveQuery = cmdFactory();
                retrieveQuery.CommandText = $"""
                    SELECT script_name
                    FROM   {_schema}.schema_versions
                    ORDER BY script_name
                    """;

                using var reader = retrieveQuery.ExecuteReader();

                return ReadAll(reader, r => r.GetString(0)).ToArray();
            });
        }

        public void StoreExecutedScript(SqlScript script, Func<IDbCommand> dbCommandFactory)
        {
            EnsureTableExistsAndIsLatestVersion(dbCommandFactory);

            using var insertQuery = dbCommandFactory();
            insertQuery.CommandText = $"""
                INSERT INTO {_schema}.schema_versions (script_name, applied)
                VALUES (@script_name, CURRENT_TIMESTAMP)
                """;

            var scriptParameter = insertQuery.CreateParameter();
            scriptParameter.ParameterName = "script_name";
            scriptParameter.Value = script.Name;
            insertQuery.Parameters.Add(scriptParameter);

            insertQuery.ExecuteNonQuery();
        }

        private static IEnumerable<T> ReadAll<T>(IDataReader reader, Func<IDataReader, T> read)
        {
            while (reader.Read())
            {
                yield return read(reader);
            }
        }
    }
}
