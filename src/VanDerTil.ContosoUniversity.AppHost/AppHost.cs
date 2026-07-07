using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Postgres
var databaseServer = builder
    .AddPostgres("pgsql")
    .WithImageTag("18");

databaseServer
    .WithPgWeb(opts => opts.WithParentRelationship(databaseServer));

var database = databaseServer.AddDatabase("contosodb", "contoso");

var migrations = builder.AddProject<Projects.VanDerTil_ContosoUniversity_Migrations>("migrations")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Seeding__Enabled", "true")
    .WithParentRelationship(database);

builder.AddProject<Projects.VanDerTil_ContosoUniversity_Web>("web")
    .WaitForCompletion(migrations);

builder.Build().Run();
