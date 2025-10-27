using Microsoft.AspNetCore.Mvc;



[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (succeeded, response, errors) = await _authService.RegisterUserAsync(model);

        if (!succeeded)
        {
            return BadRequest(new { errors = errors!.Select(e => e.Description) });
        }

        return Ok(response);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (succeeded, response) = await _authService.LoginUserAsync(model);

        if (!succeeded) return Unauthorized(new { message = "Credenciales inválidas" });

        return Ok(response);
    }
}
