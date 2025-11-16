using System.ComponentModel.DataAnnotations;

namespace SafeVault.Core.Models;

public class UserInput
{
    [Required]
    [StringLength(100)]
    [RegularExpression("^[A-Za-z0-9_.-]+$", ErrorMessage = "O nome de usuário contém caracteres inválidos.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}
