using System.Diagnostics;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using AdminModule.Keycloak;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AdminModule.Administrator.Endpoints;

internal static class Register
{
    internal static async Task<IResult> Handle(
        string token,
        IAdministratorRepository repository,
        IKeycloakAdminClient keycloakClient,
        IConfiguration configuration,
        CancellationToken ct
    )
    {
        var frontendUrl = configuration["Frontend:AdminUrl"] ?? "http://localhost:5174";

        var administrator = await repository.GetByInvitationTokenAsync(token, ct);
        if (administrator is null)
        {
            Activity.Current?.SetTag("registration.outcome", "not_found");
            return TypedResults.Redirect($"{frontendUrl}/register/not-found");
        }

        Activity.Current?.SetTag("administrator.id", administrator.Id.ToString());
        Activity.Current?.SetTag("administrator.email", administrator.Email);

        if (administrator.Status is AdministratorActive)
        {
            Activity.Current?.SetTag("registration.outcome", "already_used");
            return TypedResults.Redirect($"{frontendUrl}/register/already-used");
        }

        bool isExpired =
            administrator.Status is AdministratorPendingExpired
            || administrator.InvitationExpiresAt < DateTime.UtcNow;

        if (isExpired)
        {
            if (administrator.Status is AdministratorPending)
            {
                var expired = administrator.ExpireRegistration();
                await repository.UpdateAsync(expired, ct);
            }

            Activity.Current?.SetTag("registration.outcome", "expired");
            return TypedResults.Redirect($"{frontendUrl}/register/expired");
        }

        var keycloakUserId = await keycloakClient.CreateUserAsync(
            administrator.Email,
            administrator.FirstName,
            administrator.LastName,
            ct
        );

        await keycloakClient.AssignRealmRoleAsync(keycloakUserId, "administrator", ct);
        await keycloakClient.SendUpdatePasswordEmailAsync(
            keycloakUserId,
            $"{frontendUrl}/callback",
            ct
        );

        var activated = administrator.Register(keycloakUserId);
        await repository.UpdateAsync(activated, ct);

        Activity.Current?.SetTag("registration.outcome", "success");
        return TypedResults.Redirect($"{frontendUrl}/register/complete");
    }
}
