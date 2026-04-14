using System.ComponentModel.DataAnnotations;

namespace AdminModule.Contracts.Administrator;

public record InviteAdministratorRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(255)] string Email
);
