using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core.Models;
using SafeVault.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace SafeVault.App.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IUserRepository _repo;
    public RegisterModel(IUserRepository repo) => _repo = repo;

    [BindProperty]
    public UserInput Form { get; set; } = new();

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? PasswordError { get; set; }
    public string? ResultMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password == null)
            return BadRequest("Password must be set");
        if (Password != ConfirmPassword)
        {
            PasswordError = "Passwords do not match.";
            return Page();
        }
        if (Password.Length < 8 || !Password.Any(char.IsDigit) || !Password.Any(char.IsUpper))
        {
            PasswordError = "Password must be at least 8 chars, contain an upper-case letter and a digit.";
            return Page();
        }
        if (!ModelState.IsValid)
        {
            ResultMessage = "Invalid data.";
            return Page();
        }

        try
        {
            var user = new User { Username = Form.Username, Email = Form.Email, Role = "user" };
            await _repo.CreateWithPasswordAsync(user, Password);
            ResultMessage = "User registered. You can now login.";
        }
        catch (Exception ex)
        {
            ResultMessage = ex.Message.Contains("UNIQUE") ? "Username already exists." : "Error registering user.";
        }
        return Page();
    }
}
