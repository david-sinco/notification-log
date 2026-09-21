using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Application.Users.Commands.RequestSignInCode;
using NotificationLog.IdentityService.Application.Users.Commands.ResendVerificationCode;
using NotificationLog.IdentityService.Application.Users.Commands.SignIn;
using NotificationLog.IdentityService.Application.Users.Commands.SignInWithCode;
using NotificationLog.IdentityService.Application.Users.Dtos;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class LoginModel : PageModel
{
    public const string CodeMode = "code";

    private readonly SignInHandler _signIn;
    private readonly SignInWithCodeHandler _signInWithCode;
    private readonly RequestSignInCodeHandler _signInCodes;
    private readonly ResendVerificationCodeHandler _verificationCodes;

    public LoginModel(
        SignInHandler signIn,
        SignInWithCodeHandler signInWithCode,
        RequestSignInCodeHandler signInCodes,
        ResendVerificationCodeHandler verificationCodes)
        => (_signIn, _signInWithCode, _signInCodes, _verificationCodes)
            = (signIn, signInWithCode, signInCodes, verificationCodes);

    [BindProperty(SupportsGet = true)]
    public string Identifier { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string Code { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? Mode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? Message { get; set; }

    public string? Error { get; private set; }

    public bool IsCodeMode => Mode == CodeMode;

    public bool CodeSent => IsCodeMode && !string.IsNullOrWhiteSpace(Identifier);

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var attempt = await _signIn.HandleAsync(new SignInCommand(Identifier, Password), ct);

        return await CompleteAsync(attempt, "El correo, el teléfono o la contraseña no son correctos.", ct);
    }

    public async Task<IActionResult> OnPostSendCodeAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Identifier))
        {
            Error = "Escribe el correo o el celular de tu cuenta.";
            return Page();
        }

        await _signInCodes.HandleAsync(new RequestSignInCodeCommand(Identifier), ct);

        Message = "Si la cuenta existe, te enviamos un código.";

        return RedirectToPage("Login", new { mode = CodeMode, identifier = Identifier, returnUrl = ReturnUrl });
    }

    public async Task<IActionResult> OnPostCodeAsync(CancellationToken ct)
    {
        var attempt = await _signInWithCode.HandleAsync(new SignInWithCodeCommand(Identifier, Code), ct);

        return await CompleteAsync(attempt, "El código no es válido o ya venció.", ct);
    }

    private async Task<IActionResult> CompleteAsync(SignInAttempt attempt, string invalidMessage, CancellationToken ct)
    {
        switch (attempt.Status)
        {
            case SignInStatus.Succeeded:
                await HttpContext.SignInAsync(SessionCookie.Scheme, OidcPrincipalFactory.CreateSession(attempt.User!));
                return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");

            case SignInStatus.NotVerified:
                await _verificationCodes.HandleAsync(new ResendVerificationCodeCommand(Identifier), ct);
                return RedirectToPage("Verify", new { identifier = Identifier, returnUrl = ReturnUrl });

            case SignInStatus.LockedOut:
                Error = "Tu cuenta está bloqueada. Intenta de nuevo más tarde.";
                return Page();

            default:
                Error = invalidMessage;
                return Page();
        }
    }
}
