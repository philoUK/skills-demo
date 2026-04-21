using System.Diagnostics;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using AdminModule.Email;
using Microsoft.AspNetCore.Http;

namespace AdminModule.Administrator.Endpoints;

internal static class ResendInvitation
{
    internal static async Task<IResult> Handle(
        Guid id,
        IAdministratorRepository repository,
        IEmailService emailService,
        CancellationToken ct
    )
    {
        var administrator = await repository.GetByIdAsync(id, ct);
        if (administrator is null)
            return TypedResults.NotFound();

        Activity.Current?.SetTag("administrator.id", id);
        Activity.Current?.SetTag("administrator.email", administrator.Email);

        var result = administrator.ResendInvitation();

        if (result is Error<Domain.Administrator> error)
        {
            Activity.Current?.SetTag(
                "administrator.resend_invitation.error",
                string.Join(", ", error.Errors)
            );
            return TypedResults.Conflict(string.Join(", ", error.Errors));
        }

        var updated = ((Ok<Domain.Administrator>)result).Value;
        await repository.UpdateAsync(updated, ct);

        await emailService.SendInvitationAsync(
            updated.Email,
            updated.FirstName,
            updated.InvitationToken!.Value,
            ct
        );

        return TypedResults.NoContent();
    }
}
