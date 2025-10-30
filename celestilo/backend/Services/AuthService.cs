using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


public class AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtSettings jwtSettings, ILogger<AuthService> logger) : IAuthService
{
    public async Task<(bool Succeeded, AuthResponse? Response, IEnumerable<IdentityError>? Errors)> RegisterUserAsync(RegisterModel model)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            logger.LogDebug("Starting registration - CorrelationId: {CorrelationId}", correlationId);

            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errorCodes = string.Join(", ", result.Errors.Select(e => e.Code));
                logger.LogWarning("Failure to create user {UserName}. Errors: {ErrorCodes}, CorrelationId: {CorrelationId}", model.UserName, errorCodes, correlationId);
                return (false, null, result.Errors);
            }

            await userManager.AddToRoleAsync(user, "User");

            var roles = await userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, roles);

            var response = new AuthResponse
            {
                Token = token,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles
            };
            logger.LogInformation("Registration successful. UserId: {UserId}, Roles: {RoleCount}, CorrelationId: {CorrelationId}", user.Id, roles.Count, correlationId);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error during registration: {UserName}, CorrelationId: {CorrelationId}", model.UserName, correlationId);
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
            logger.LogDebug("Starting login - CorrelationId: {CorrelationId}", correlationId);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                logger.LogWarning("Login failed - CorrelationId: {CorrelationId}", correlationId);

                // Delay artificial para prevenir timing attacks
                await Task.Delay(Random.Shared.Next(100, 300));
                return (false, null);
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false); // No lockout for simplicity in development

            if (!result.Succeeded)
            {
                var reason = result.IsLockedOut ? "Lockout" : result.IsNotAllowed ? "NotAllowed" : "InvalidCredentials";

                logger.LogWarning("Login failed. Reason: {Reason}, UserId: {UserId}, CorrelationId: {CorrelationId}", reason, user.Id, correlationId);
                await Task.Delay(Random.Shared.Next(100, 300));

                return (false, null);
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            var response = new AuthResponse
            {
                Token = token,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles
            };

            logger.LogInformation("Login successful. UserId: {UserId}, Roles: {RoleCount}, CorrelationId: {CorrelationId}", user.Id, roles.Count, correlationId);

            return (true, response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error during login. CorrelationId: {CorrelationId}", correlationId);
            return (false, null);
        }
    }
    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
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
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
