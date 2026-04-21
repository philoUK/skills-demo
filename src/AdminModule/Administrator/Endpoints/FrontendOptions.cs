namespace AdminModule.Administrator.Endpoints;

internal sealed record FrontendOptions
{
    public required string AdminUrl { get; init; }
}
