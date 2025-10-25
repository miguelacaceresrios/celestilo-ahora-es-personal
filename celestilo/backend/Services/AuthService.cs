using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    // Podrías inyectar IConfiguration para leer los valores del JWT en lugar de Environment
    public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterUserAsync(RegisterModel model)
    {
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Asignar rol "User" por defecto
            await _userManager.AddToRoleAsync(user, "User");
        }

        return result;
    }

    public async Task<(bool Succeeded, AuthResponse? Response)> LoginUserAsync(LoginModel model)
    {
        // Verificar si el usuario existe mediante el email y devuelve null si no existe o IdentityUser si existe
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user is null) return(false, null);

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        
        if (!result.Succeeded) return (false, null);
        
        var roles = await _userManager.GetRolesAsync(user);
        var token = await GenerateJwtToken(user, roles);

        var response = new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return (true, response);
    }
    //estudiar esto
    private async Task<string> GenerateJwtToken(IdentityUser user, IList<string> roles )
    {
        var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!;
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
        var jwtExpiration = Convert.ToInt32(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
       
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpiration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
