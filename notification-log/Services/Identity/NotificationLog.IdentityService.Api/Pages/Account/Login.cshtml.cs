using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Connect;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly AccountService _accounts;

    public LoginModel(AccountService accounts) => _accounts = accounts;

    [BindProperty]
    public string Identifier { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? Message { get; set; }

    public string? Error { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var attempt = await _accounts.SignInAsync(Identifier, Password, ct);

        switch (attempt.Status)
        {
            case SignInStatus.Succeeded:
                await HttpContext.SignInAsync(SessionCookie.Scheme, OidcPrincipalFactory.CreateSession(attempt.User!));
                return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");

            case SignInStatus.NotVerified:
                await _accounts.ResendCodeAsync(Identifier, ct);
                return RedirectToPage("Verify", new { identifier = Identifier, returnUrl = ReturnUrl });

            case SignInStatus.LockedOut:
                Error = "Tu cuenta está bloqueada. Intenta de nuevo más tarde.";
                return Page();

            default:
                Error = "El correo, el teléfono o la contraseña no son correctos.";
                return Page();
        }
    }
}
