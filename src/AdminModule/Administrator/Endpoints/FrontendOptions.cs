namespace AdminModule.Administrator.Endpoints;

internal sealed record FrontendOptions
{
    public string AdminUrl { get; init; } = "http://localhost:5174";
}
