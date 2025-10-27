
using Microsoft.AspNetCore.Identity;

public interface IAuthService
{
    Task<(bool Succeeded, AuthResponse? Response, IEnumerable<IdentityError>? Errors)> RegisterUserAsync(RegisterModel model);
    Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model);
}
