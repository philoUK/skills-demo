namespace AdminModule.Email;

internal sealed record ApiOptions
{
    public required string BaseUrl { get; init; }
}
