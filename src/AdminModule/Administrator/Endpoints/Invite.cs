using System.Diagnostics;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using AdminModule.Contracts.Administrator;
using AdminModule.Email;
using Microsoft.AspNetCore.Http;

namespace AdminModule.Administrator.Endpoints;

internal static class Invite
{
    internal static async Task<IResult> Handle(
        InviteAdministratorRequest request,
        IAdministratorRepository repository,
        IEmailService emailService,
        CancellationToken ct
    )
    {
        var existing = await repository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            return TypedResults.Conflict(
                "An administrator with this email address already exists."
            );

        var administrator = AdministratorFactory.Invite(
            request.Email,
            request.FirstName,
            request.LastName
        );

        await repository.CreateAsync(administrator, ct);

        Activity.Current?.SetTag("administrator.email", request.Email);
        Activity.Current?.SetTag(
            "invitation.expires_at",
            administrator.InvitationExpiresAt?.ToString("O")
        );
        Activity.Current?.SetTag("invitation.token", administrator.InvitationToken);

        await emailService.SendInvitationAsync(
            request.Email,
            request.FirstName,
            administrator.InvitationToken!,
            ct
        );

        return TypedResults.NoContent();
    }
}
