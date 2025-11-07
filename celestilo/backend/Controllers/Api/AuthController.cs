using Microsoft.AspNetCore.Mvc;
using backend.Services.Interfaces;
using backend.Model.Auth;
namespace backend.Controllers.Api;

/// <summary>
/// Controller responsible for handling authentication operations such as user registration and login.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="model">The registration model containing username, email, and password.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if the model state is invalid or registration fails.
    /// - 200 OK with an <see cref="AuthResponse"/> containing the JWT token and user information upon successful registration.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (succeeded, response, errors) = await authService.RegisterUserAsync(model);

        if (!succeeded)
        {
            return BadRequest(new { errors = errors!.Select(e => e.Description) });
        }

        return Ok(response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="model">The login model containing email and password.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if the model state is invalid.
    /// - 401 Unauthorized if the credentials are invalid.
    /// - 200 OK with an <see cref="AuthResponse"/> containing the JWT token and user information upon successful authentication.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (succeeded, response) = await authService.LoginUserAsync(model);

        if (!succeeded) return Unauthorized(new { message = "Credenciales inválidas" });

        return Ok(response);
    }
}
