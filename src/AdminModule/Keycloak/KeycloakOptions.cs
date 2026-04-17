namespace AdminModule.Keycloak;

internal sealed record KeycloakOptions
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public required string AdminClientId { get; init; }
    public required string AdminClientSecret { get; init; }
}
