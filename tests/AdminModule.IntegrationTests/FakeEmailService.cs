using AdminModule.Email;

namespace AdminModule.IntegrationTests;

internal class FakeEmailService : IEmailService
{
    private readonly List<(string To, string Subject, string Body)> _sent = [];

    public IReadOnlyList<(string To, string Subject, string Body)> Sent => _sent;

    public void Clear() => _sent.Clear();

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _sent.Add((to, subject, body));
        return Task.CompletedTask;
    }

    public Task SendInvitationAsync(string email, string firstName, string token, CancellationToken ct = default) =>
        SendAsync(email, "You have been invited to Fizz", $"/admin/register/{token}", ct);
}
