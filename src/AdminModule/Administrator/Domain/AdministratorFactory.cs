namespace AdminModule.Administrator.Domain;

internal static class AdministratorFactory
{
    internal static Administrator Invite(string email, string firstName, string lastName) =>
        new(
            Guid.NewGuid(),
            email,
            firstName,
            lastName,
            AdministratorStatusFactory.Pending(),
            null,
            InvitationToken.Generate(),
            DateTime.UtcNow.AddHours(24),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

    internal static Administrator Load(
        Guid id,
        string email,
        string firstName,
        string lastName,
        AdministratorStatus status,
        string? keycloakUserId,
        InvitationToken? invitationToken,
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
