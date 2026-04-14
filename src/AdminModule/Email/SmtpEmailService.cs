using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace AdminModule.Email;

internal class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    private readonly string _host = configuration["Smtp:Host"] ?? "localhost";
    private readonly int _port = int.TryParse(configuration["Smtp:Port"], out var port)
        ? port
        : 1025;
    private readonly string _apiBaseUrl = configuration["Api:BaseUrl"] ?? "http://localhost:5000";

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken ct = default
    )
    {
        using var client = new SmtpClient(_host, _port) { EnableSsl = false };
        using var message = new MailMessage("noreply@fizz.local", to, subject, body)
        {
            IsBodyHtml = true,
        };
        await client.SendMailAsync(message, ct);
    }

    public Task SendInvitationAsync(string email, string firstName, string token, CancellationToken ct = default)
    {
        var registrationLink = $"{_apiBaseUrl}/admin/register/{token}";
        var subject = "You have been invited to Fizz";
        var body = $"""
            <p>Hi {firstName},</p>
            <p>You have been invited to join Fizz as an administrator.</p>
            <p><a href="{registrationLink}">Complete your registration</a></p>
            <p>This link expires in 24 hours.</p>
            """;
        return SendAsync(email, subject, body, ct);
    }
}
