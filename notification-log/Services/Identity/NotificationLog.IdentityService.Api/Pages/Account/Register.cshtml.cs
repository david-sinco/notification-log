using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Api.Accounts;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class RegisterModel : PageModel
{
    private readonly AccountService _accounts;

    public RegisterModel(AccountService accounts) => _accounts = accounts;

    [BindProperty]
    public string Identifier { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? Error { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (Password != ConfirmPassword)
        {
            Error = "Las contraseñas no coinciden.";
            return Page();
        }

        var result = await _accounts.RegisterAsync(Identifier, Password, ct);

        if (!result.Succeeded)
        {
            Error = result.Error;
            return Page();
        }

        return RedirectToPage("Verify", new { identifier = Identifier, returnUrl = ReturnUrl });
    }
}
