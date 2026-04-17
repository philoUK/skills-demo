namespace AdminModule.Email;

internal sealed record SmtpOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1025;
}
