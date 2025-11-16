using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core.Services;

namespace SafeVault.App.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IUserRepository _repo;
    public LoginModel(IUserRepository repo) => _repo = repo;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Username and password are required.";
            return Page();
        }

        if (!await _repo.ValidateCredentialsAsync(Username, Password))
        {
            Error = "Invalid credentials.";
            return Page();
        }

        var user = await _repo.GetUserForAuthAsync(Username);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user!.Username),
            new Claim(ClaimTypes.Role, user.Role ?? "user")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Redireciona conforme role
        if (user.Role == "admin")
            return RedirectToPage("/Admin/Dashboard");
        return RedirectToPage("/Dashboard/User");
    }
}
