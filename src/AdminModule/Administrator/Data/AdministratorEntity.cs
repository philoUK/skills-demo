using AdminModule.Administrator.Domain;

namespace AdminModule.Administrator.Data;

internal class AdministratorEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? KeycloakUserId { get; set; }
    public string? InvitationToken { get; set; }
    public DateTime? InvitationExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    internal Domain.Administrator ToDomain() =>
        AdministratorFactory.Load(
            Id,
            Email,
            FirstName,
            LastName,
            AdministratorStatusFactory.FromString(Status),
            KeycloakUserId,
            InvitationToken,
            InvitationExpiresAt,
            CreatedAt,
            UpdatedAt
        );

    internal static AdministratorEntity FromDomain(Domain.Administrator a) =>
        new()
        {
            Id = a.Id,
            Email = a.Email,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Status = AdministratorStatusFactory.ToStorageString(a.Status),
            KeycloakUserId = a.KeycloakUserId,
            InvitationToken = a.InvitationToken,
            InvitationExpiresAt = a.InvitationExpiresAt,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
        };
}
