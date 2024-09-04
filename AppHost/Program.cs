using Projects;
var builder = DistributedApplication.CreateBuilder(args);

var eventStoreDb = builder
    .AddContainer("esdb-node", "eventstore/eventstore")
    .WithHttpEndpoint(port: 2113, targetPort: 2113, "http")
    .WithArgs("--insecure")
    .WithArgs("--run-projections=All")
    .WithArgs("--enable-atom-pub-over-http");

builder.AddProject<WebApi>("webapi")
    // .WithEnvironment("ConnectionStrings:EventStoreDB", eventStoreDb.GetEndpoint("http"))
    .WithExternalHttpEndpoints();

builder.Build().Run();