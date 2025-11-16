using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core.Models;
using SafeVault.Core.Services;

namespace SafeVault.App.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IUserRepository _repo;
    public DashboardModel(IUserRepository repo) => _repo = repo;

    public List<User> Users { get; set; } = new();

    public async Task OnGet()
    {
        var all = await _repo.ListAllAsync();
        Users = all.ToList();
    }
}
