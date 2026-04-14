namespace AdminModule.Email;

internal interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);

    Task SendInvitationAsync(string email, string firstName, string token, CancellationToken ct = default);
}
