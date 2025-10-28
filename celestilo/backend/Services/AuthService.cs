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
    public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtSettings jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings;
    }

    public async Task<(bool Succeeded, AuthResponse? Response, IEnumerable<IdentityError>? Errors)> RegisterUserAsync(RegisterModel model)
    {
        var user = new IdentityUser
        {
            UserName = model.UserName,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded) return (false, null, result.Errors);

        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);

        var token = GenerateJwtToken(user, roles);

        var response = new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return (true, response, null);
    }

    public async Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null) return (false, null);

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        if (!result.Succeeded) return (false, null);

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        var response = new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return (true, response);
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
