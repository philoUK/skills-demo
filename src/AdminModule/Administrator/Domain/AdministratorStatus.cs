namespace AdminModule.Administrator.Domain;

internal abstract record AdministratorStatus;

internal record AdministratorActive : AdministratorStatus;

internal record AdministratorInactive : AdministratorStatus;

internal record AdministratorPending : AdministratorStatus;

internal record AdministratorPendingExpired : AdministratorStatus;

internal static class AdministratorStatusFactory
{
    public static AdministratorStatus Active() => new AdministratorActive();

    public static AdministratorStatus Inactive() => new AdministratorInactive();

    public static AdministratorStatus Pending() => new AdministratorPending();

    public static AdministratorStatus PendingExpired() => new AdministratorPendingExpired();

    public static AdministratorStatus FromString(string value) =>
        value switch
        {
            "active" => new AdministratorActive(),
            "inactive" => new AdministratorInactive(),
            "pending" => new AdministratorPending(),
            "pending_expired" => new AdministratorPendingExpired(),
            _ => throw new ArgumentException(
                $"Unknown administrator status: {value}",
                nameof(value)
            ),
        };

    public static string ToStorageString(AdministratorStatus status) =>
        status switch
        {
            AdministratorActive => "active",
            AdministratorInactive => "inactive",
            AdministratorPending => "pending",
            AdministratorPendingExpired => "pending_expired",
            _ => throw new ArgumentException(
                $"Unknown administrator status type: {status.GetType().Name}"
            ),
        };
}
