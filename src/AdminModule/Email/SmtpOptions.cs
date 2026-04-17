namespace AdminModule.Email;

internal sealed record SmtpOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
}
