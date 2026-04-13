namespace AdminModule.Administrator.Domain;

internal record Administrator(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    AdministratorStatus Status,
    string? KeycloakUserId,
    string? InvitationToken,
    DateTime? InvitationExpiresAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
