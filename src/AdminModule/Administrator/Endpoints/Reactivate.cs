using System.Diagnostics;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using Microsoft.AspNetCore.Http;

namespace AdminModule.Administrator.Endpoints;

internal static class Reactivate
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
        Activity.Current?.SetTag(
            "administrator.status",
            AdministratorStatusFactory.ToStorageString(administrator.Status)
        );

        var result = administrator.Reactivate();

        if (result is Error<Domain.Administrator> error)
        {
            Activity.Current?.SetTag("administrator.reactivate.error", string.Join(", ", error.Errors));
            return TypedResults.Conflict(string.Join(", ", error.Errors));
        }

        var updated = ((Ok<Domain.Administrator>)result).Value;
        await repository.UpdateAsync(updated, ct);

        Activity.Current?.SetTag("administrator.reactivated", true);

        return TypedResults.NoContent();
    }
}
