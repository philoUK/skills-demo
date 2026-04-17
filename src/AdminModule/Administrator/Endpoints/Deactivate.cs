using System.Diagnostics;
using System.Security.Claims;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using Microsoft.AspNetCore.Http;

namespace AdminModule.Administrator.Endpoints;

internal static class Deactivate
{
    internal static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal user,
        IAdministratorRepository repository,
        CancellationToken ct
    )
    {
        var administrator = await repository.GetByIdAsync(id, ct);

        if (administrator is null)
            return TypedResults.NotFound();

        Activity.Current?.SetTag("administrator.id", id);
        Activity.Current?.SetTag(
            "administrator.status",
            AdministratorStatusFactory.ToStorageString(administrator.Status)
        );

        var currentUserKeycloakId = user.FindFirst("sub")?.Value;
        if (
            currentUserKeycloakId is not null
            && administrator.KeycloakUserId == currentUserKeycloakId
        )
        {
            Activity.Current?.SetTag("administrator.self_deactivation_attempted", true);
            return TypedResults.Conflict("Cannot deactivate your own account.");
        }

        var result = administrator.Deactivate();

        if (result is Error<Domain.Administrator> error)
        {
            Activity.Current?.SetTag(
                "administrator.deactivate.error",
                string.Join(", ", error.Errors)
            );
            return TypedResults.Conflict(string.Join(", ", error.Errors));
        }

        var updated = ((Ok<Domain.Administrator>)result).Value;
        await repository.UpdateAsync(updated, ct);

        Activity.Current?.SetTag("administrator.deactivated", true);

        return TypedResults.NoContent();
    }
}
