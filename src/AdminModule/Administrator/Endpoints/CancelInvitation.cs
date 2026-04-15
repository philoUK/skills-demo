using System.Diagnostics;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using Microsoft.AspNetCore.Http;

namespace AdminModule.Administrator.Endpoints;

internal static class CancelInvitation
{
    internal static async Task<IResult> Handle(
        Guid id,
        IAdministratorRepository repository,
        CancellationToken ct
    )
    {
        var administrator = await repository.GetByIdAsync(id, ct);
        if (administrator is null)
            return TypedResults.NotFound();

        Activity.Current?.SetTag("administrator.id", id);
        Activity.Current?.SetTag("administrator.email", administrator.Email);

        if (administrator.Status is not (AdministratorPending or AdministratorPendingExpired))
        {
            Activity.Current?.SetTag(
                "administrator.cancel_invitation.error",
                "Administrator invitation cannot be cancelled."
            );
            return TypedResults.Conflict("Administrator invitation cannot be cancelled.");
        }

        await repository.DeleteAsync(id, ct);

        return TypedResults.NoContent();
    }
}
