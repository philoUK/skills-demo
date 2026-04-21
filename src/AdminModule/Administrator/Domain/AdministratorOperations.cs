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
}
