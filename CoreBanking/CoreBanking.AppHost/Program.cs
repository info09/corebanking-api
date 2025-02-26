var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImageTag("latest")
    .WithVolume("corebanking-data", "/var/lib/postgresql/data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(builder =>
    {
        builder.WithImageTag("latest");
    });

var corebankingDb = postgres.AddDatabase("corebanking-db", "corebanking");
var migrationService = builder.AddProject<Projects.CoreBanking_MigrationService>("corebanking-migrationservice");

builder.AddProject<Projects.CoreBanking_API>("corebanking-api")
    .WithReference(corebankingDb)
    .WaitFor(postgres)
    .WaitForCompletion(migrationService);



builder.Build().Run();
