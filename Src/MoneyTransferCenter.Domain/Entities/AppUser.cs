using Microsoft.AspNetCore.Identity;

namespace MoneyTransferCenter.Domain.Entities;

public sealed class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string FullName => $"{FirstName} {LastName}";
    // Navigation property
    public Account? Account { get; set; }
}
