using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Application.Users.Commands.ResendVerificationCode;
using NotificationLog.IdentityService.Application.Users.Commands.SignIn;
using NotificationLog.IdentityService.Application.Users.Dtos;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly SignInHandler _signIn;
    private readonly ResendVerificationCodeHandler _codes;

    public LoginModel(SignInHandler signIn, ResendVerificationCodeHandler codes) => (_signIn, _codes) = (signIn, codes);

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
        var attempt = await _signIn.HandleAsync(new SignInCommand(Identifier, Password), ct);

        switch (attempt.Status)
        {
            case SignInStatus.Succeeded:
                await HttpContext.SignInAsync(SessionCookie.Scheme, OidcPrincipalFactory.CreateSession(attempt.User!));
                return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");

            case SignInStatus.NotVerified:
                await _codes.HandleAsync(new ResendVerificationCodeCommand(Identifier), ct);
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
