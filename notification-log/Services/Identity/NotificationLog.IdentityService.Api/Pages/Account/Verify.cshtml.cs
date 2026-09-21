using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Application.Users.Commands.ResendVerificationCode;
using NotificationLog.IdentityService.Application.Users.Commands.VerifyAccount;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class VerifyModel : PageModel
{
    private readonly VerifyAccountHandler _verify;
    private readonly ResendVerificationCodeHandler _codes;

    public VerifyModel(VerifyAccountHandler verify, ResendVerificationCodeHandler codes)
        => (_verify, _codes) = (verify, codes);

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
        var result = await _verify.HandleAsync(new VerifyAccountCommand(Identifier, Code), ct);

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
        await _codes.HandleAsync(new ResendVerificationCodeCommand(Identifier), ct);
        Message = "Te enviamos un código nuevo.";

        return Page();
    }
}
