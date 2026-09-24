using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Application.Users.Commands.RegisterAccount;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Api.Pages.Account;

public sealed class RegisterModel : PageModel
{
    private readonly RegisterAccountHandler _register;
    private readonly RegistrationRoleResolver _roles;

    public RegisterModel(RegisterAccountHandler register, RegistrationRoleResolver roles)
        => (_register, _roles) = (register, roles);

    [BindProperty]
    public string Identifier { get; set; } = string.Empty;

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Locale { get; set; } = AccountPolicy.DefaultLocale;

    [BindProperty]
    public string TimeZone { get; set; } = AccountPolicy.DefaultTimeZone;

    [BindProperty]
    public bool AcceptsNotifications { get; set; }

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

        var role = await _roles.ResolveAsync(ReturnUrl, ct);

        var result = await _register.HandleAsync(
            new RegisterAccountCommand(Identifier, Password, Name, Locale, TimeZone, AcceptsNotifications, role), ct);

        if (!result.Succeeded)
        {
            Error = result.Error;
            return Page();
        }

        return RedirectToPage("Verify", new { identifier = Identifier, returnUrl = ReturnUrl });
    }
}
