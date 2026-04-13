namespace AdminModule.Administrator.Domain;

internal static class AdministratorFactory
{
    internal static Administrator Load(
        Guid id,
        string email,
        string firstName,
        string lastName,
        AdministratorStatus status,
        string? keycloakUserId,
        string? invitationToken,
        DateTime? invitationExpiresAt,
        DateTime createdAt,
        DateTime updatedAt
    ) =>
        new(
            id,
            email,
            firstName,
            lastName,
            status,
            keycloakUserId,
            invitationToken,
            invitationExpiresAt,
            createdAt,
            updatedAt
        );
}
