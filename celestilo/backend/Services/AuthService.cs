using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtSettings jwtSettings, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings;
        _logger = logger;

    }

    public async Task<(bool Succeeded, AuthResponse? Response, IEnumerable<IdentityError>? Errors)> RegisterUserAsync(RegisterModel model)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            _logger.LogDebug("Starting registration - CorrelationId: {CorrelationId}", correlationId);

            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errorCodes = string.Join(", ", result.Errors.Select(e => e.Code));
                _logger.LogWarning("Failure to create user {UserName}. Errors: {ErrorCodes}, CorrelationId: {CorrelationId}", model.UserName, errorCodes, correlationId);
                return (false, null, result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, roles);

            var response = new AuthResponse
            {
                Token = token,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles
            };
            _logger.LogInformation("Registration successful. UserId: {UserId}, Roles: {RoleCount}, CorrelationId: {CorrelationId}", user.Id, roles.Count, correlationId);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical error during registration: {UserName}, CorrelationId: {CorrelationId}", model.UserName, correlationId);
            var errors = new List<IdentityError>
            {
                new()
                {
                    Code  = "RegistrationError",
                    Description = "An error occurred during registration. Please try again."
                }
            };
            return (false, null, errors);
        }
    }

    public async Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogDebug("Starting login - CorrelationId: {CorrelationId}", correlationId);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                _logger.LogWarning("Login failed - CorrelationId: {CorrelationId}", correlationId);

                // Delay artificial para prevenir timing attacks
                await Task.Delay(Random.Shared.Next(100, 300));
                return (false, null);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false); // No lockout for simplicity in development

            if (!result.Succeeded)
            {
                var reason = result.IsLockedOut ? "Lockout" : result.IsNotAllowed ? "NotAllowed" : "InvalidCredentials";

                _logger.LogWarning("Login failed. Reason: {Reason}, UserId: {UserId}, CorrelationId: {CorrelationId}", reason, user.Id, correlationId);
                await Task.Delay(Random.Shared.Next(100, 300));

                return (false, null);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            var response = new AuthResponse
            {
                Token = token,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles
            };

            _logger.LogInformation("Login successful. UserId: {UserId}, Roles: {RoleCount}, CorrelationId: {CorrelationId}", user.Id, roles.Count, correlationId);

            return (true, response);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical error during login. CorrelationId: {CorrelationId}", correlationId);
            return (false, null);
        }
    }
    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName!)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
