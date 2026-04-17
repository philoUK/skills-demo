namespace AdminModule.Administrator.Domain;

internal record Administrator(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    AdministratorStatus Status,
    string? KeycloakUserId,
    InvitationToken? InvitationToken,
    DateTime? InvitationExpiresAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
