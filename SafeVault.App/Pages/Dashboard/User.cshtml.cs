using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core.Models;
using SafeVault.Core.Services;

namespace SafeVault.App.Pages.Dashboard;

public class UserModel : PageModel
{
    private readonly IUserRepository _repo;
    public UserModel(IUserRepository repo) => _repo = repo;

    public User? CurrentUser { get; set; }
    public string? Status { get; set; }

    public async Task OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            CurrentUser = await _repo.GetUserForAuthAsync(User.Identity!.Name!);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!(User.Identity?.IsAuthenticated ?? false)) return RedirectToPage("/Account/Login");
        var username = User.Identity!.Name!;
        var newPassword = Request.Form["NewPassword"].ToString();
        var confirm = Request.Form["ConfirmNewPassword"].ToString();

        if (newPassword != confirm)
        {
            Status = "Passwords do not match.";
            await OnGet();
            return Page();
        }
        if (newPassword.Length < 8 || !newPassword.Any(char.IsDigit) || !newPassword.Any(char.IsUpper))
        {
            Status = "Password must be at least 8 chars, contain an upper-case letter and a digit.";
            await OnGet();
            return Page();
        }

        await _repo.UpdatePasswordAsync(username, newPassword);
        Status = "Password updated.";
        await OnGet();
        return Page();
    }
}
