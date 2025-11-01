using Microsoft.AspNetCore.Identity;
using backend.Model.Auth;
namespace backend.Services;

/// <summary>
/// Interface defining authentication service operations for user registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="model">The registration model containing username, email, and password.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the registration succeeded.
    /// - An <see cref="AuthResponse"/> object with JWT token and user information if successful, otherwise null.
    /// - A collection of <see cref="IdentityError"/> objects if registration failed, otherwise null.
    /// </returns>
    Task<(bool Succeeded, AuthResponse? Response, IEnumerable<IdentityError>? Errors)> RegisterUserAsync(RegisterModel model);
    
    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="model">The login model containing email and password.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the login succeeded.
    /// - An <see cref="AuthResponse"/> object with JWT token and user information if successful, otherwise null.
    /// </returns>
    Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model);
}
