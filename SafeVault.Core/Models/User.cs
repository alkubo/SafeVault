using System.ComponentModel.DataAnnotations;

namespace SafeVault.Core.Models;

public class User
{
    [Required]
    [StringLength(100)]
    [RegularExpression("^[A-Za-z0-9_.-]+$", ErrorMessage = "Username contains invalid characters.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(admin|user)$", ErrorMessage = "Invalid role.")]
    public string Role { get; set; } = "user";
}
