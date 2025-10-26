
using Microsoft.AspNetCore.Identity;

public interface IAuthService
{
    Task<IdentityResult> RegisterUserAsync(RegisterModel model);
    Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model);
}
