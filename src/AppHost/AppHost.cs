var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak").WithRealmImport("../../keycloak");
var keycloakHttp = keycloak.GetEndpoint("http");

var postgres = builder.AddPostgres("postgres");
var adminDb = postgres.AddDatabase("admindb");

var api = builder
    .AddProject<Projects.Api>("api")
    .WaitFor(keycloak)
    .WaitFor(postgres)
    .WithReference(adminDb)
    .WithEnvironment(
        "Keycloak__Authority",
        ReferenceExpression.Create($"{keycloakHttp}/realms/fizz")
    )
    .WithEnvironment("Keycloak__Audience", "adminweb");

var apiHttp = api.GetEndpoint("http");

builder
    .AddNpmApp("adminweb", "../adminweb", "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WaitFor(keycloak)
    .WaitFor(api)
    .WithEnvironment("VITE_KEYCLOAK_URL", ReferenceExpression.Create($"{keycloakHttp}"))
    .WithEnvironment("VITE_KEYCLOAK_REALM", "fizz")
    .WithEnvironment("VITE_API_URL", ReferenceExpression.Create($"{apiHttp}"));

builder
    .AddNpmApp("memberweb", "../memberweb", "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WaitFor(api);

builder.Build().Run();
