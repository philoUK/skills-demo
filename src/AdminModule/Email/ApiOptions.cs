namespace AdminModule.Email;

internal sealed record ApiOptions
{
    public string BaseUrl { get; init; } = "http://localhost:5000";
}
