namespace AdminModule.Administrator.Domain;

internal static class AdministratorOperations
{
    internal static Result<Administrator> Deactivate(this Administrator administrator) =>
        administrator.Status is AdministratorActive
            ? new Ok<Administrator>(
                administrator with
                {
                    Status = AdministratorStatusFactory.Inactive(),
                    UpdatedAt = DateTime.UtcNow,
                }
            )
            : new Error<Administrator>(["Administrator is not active."]);

    internal static Result<Administrator> Reactivate(this Administrator administrator) =>
        administrator.Status is AdministratorInactive
            ? new Ok<Administrator>(
                administrator with
                {
                    Status = AdministratorStatusFactory.Active(),
                    UpdatedAt = DateTime.UtcNow,
                }
            )
            : new Error<Administrator>(["Administrator is not inactive."]);

    internal static Administrator Register(
        this Administrator administrator,
        string keycloakUserId
    ) =>
        administrator with
        {
            Status = AdministratorStatusFactory.Active(),
            KeycloakUserId = keycloakUserId,
            InvitationToken = null,
            InvitationExpiresAt = null,
            UpdatedAt = DateTime.UtcNow,
        };

    internal static Administrator ExpireRegistration(this Administrator administrator) =>
        administrator with
        {
            Status = AdministratorStatusFactory.PendingExpired(),
            UpdatedAt = DateTime.UtcNow,
        };

    internal static Result<Administrator> ResendInvitation(this Administrator administrator)
    {
        if (administrator.Status is not (AdministratorPending or AdministratorPendingExpired))
            return new Error<Administrator>(["Administrator invitation cannot be resent."]);

        return new Ok<Administrator>(
            administrator with
            {
                Status = AdministratorStatusFactory.Pending(),
                InvitationToken = InvitationToken.Generate(),
                InvitationExpiresAt = DateTime.UtcNow.AddHours(24),
                UpdatedAt = DateTime.UtcNow,
            }
        );
    }
}
