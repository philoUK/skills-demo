using System.Diagnostics;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using AdminModule.Contracts.Administrator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HttpOk = Microsoft.AspNetCore.Http.HttpResults.Ok<AdminModule.Contracts.Administrator.ListAdministratorsResponse>;

namespace AdminModule.Administrator.Endpoints;

internal static class List
{
    internal static async Task<HttpOk> Handle(
        [FromQuery] string? search,
        IAdministratorRepository repository,
        CancellationToken ct
    )
    {
        var administrators = await repository.ListAsync(search, ct);

        Activity.Current?.SetTag("administrator.count", administrators.Count);
        Activity.Current?.SetTag("administrator.search_term", search ?? string.Empty);

        var response = new ListAdministratorsResponse(
            administrators
                .Select(a => new AdministratorResponse(
                    a.Id,
                    a.Email,
                    a.FirstName,
                    a.LastName,
                    AdministratorStatusFactory.ToStorageString(a.Status)
                ))
                .ToList()
        );

        return TypedResults.Ok(response);
    }
}
