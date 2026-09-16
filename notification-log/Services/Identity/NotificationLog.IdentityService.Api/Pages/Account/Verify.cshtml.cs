using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Api.Accounts;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class VerifyModel : PageModel
{
    private readonly AccountService _accounts;

    public VerifyModel(AccountService accounts) => _accounts = accounts;

    [BindProperty(SupportsGet = true)]
    public string Identifier { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string Code { get; set; } = string.Empty;

    public string? Message { get; private set; }

    public string? Error { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var result = await _accounts.VerifyAsync(Identifier, Code, ct);

        if (!result.Succeeded)
        {
            Error = result.Error;
            return Page();
        }

        TempData[nameof(LoginModel.Message)] = "Cuenta verificada. Ya puedes iniciar sesión.";

        return RedirectToPage("Login", new { returnUrl = ReturnUrl });
    }

    public async Task<IActionResult> OnPostResendAsync(CancellationToken ct)
    {
        await _accounts.ResendCodeAsync(Identifier, ct);
        Message = "Te enviamos un código nuevo.";

        return Page();
    }
}
