using Microsoft.AspNetCore.Identity;
using backend.Model.Auth;
namespace backend.Services;
public interface IAuthService
{
    Task<(bool Succeeded, AuthResponse? Response, IEnumerable<IdentityError>? Errors)> RegisterUserAsync(RegisterModel model);
    Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model);
}
