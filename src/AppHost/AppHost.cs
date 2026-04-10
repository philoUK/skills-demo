var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddNpmApp("adminweb", "../adminweb", "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder
    .AddNpmApp("memberweb", "../memberweb", "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
