using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core.Models;
using SafeVault.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace SafeVault.App.Pages;

public class IndexModel : PageModel
{
    private readonly IUserRepository _repo;
    public IndexModel(IUserRepository repo) => _repo = repo;

    [BindProperty]
    public UserInput FormData { get; set; } = new();

    public string? ResultMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ResultMessage = "Dados inválidos.";
            return Page();
        }

        // Username já normalizado via regex; email validado por atributo
        try
        {
            await _repo.CreateAsync(FormData);
            ResultMessage = "Usuário cadastrado com sucesso.";
        }
        catch (Exception ex)
        {
            ResultMessage = ex.Message.Contains("UNIQUE") ? "Username já existe." : "Erro ao salvar.";
        }
        return Page();
    }
}
