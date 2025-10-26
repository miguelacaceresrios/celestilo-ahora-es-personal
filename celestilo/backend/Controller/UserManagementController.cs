
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // Solo admins pueden acceder
public class UserManagementController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: api/usermanagement/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles,
                LockoutEnd = user.LockoutEnd,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }
        return Ok(userDtos);
    }

    // GET: api/usermanagement/users/{id}
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        var roles = await _userManager.GetRolesAsync(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            LockoutEnd = user.LockoutEnd,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
        };
        return Ok(userDto);
    }

    // POST: api/usermanagement/users
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // Asignar roles si se especificaron
        if (model.Roles != null && model.Roles.Any())
        {
            foreach (var role in model.Roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }
        else
        {
            // Rol por defecto
            await _userManager.AddToRoleAsync(user, "User");
        }

        return Ok(new { message = "Usuario creado exitosamente", userId = user.Id });
    }

    // PUT: api/usermanagement/users/{id}
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        // Actualizar información básica
        if (!string.IsNullOrEmpty(model.Username))
            user.UserName = model.Username;

        if (!string.IsNullOrEmpty(model.Email))
            user.Email = model.Email;

        if (model.EmailConfirmed.HasValue)
            user.EmailConfirmed = model.EmailConfirmed.Value;

        if (!string.IsNullOrEmpty(model.PhoneNumber))
            user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Usuario actualizado exitosamente" });
    }

    // DELETE: api/usermanagement/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        // No permitir eliminar al propio admin
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == id)
            return BadRequest(new { message = "No puedes eliminar tu propia cuenta" });

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Usuario eliminado exitosamente" });
    }

    // POST: api/usermanagement/users/{id}/roles
    [HttpPost("users/{id}/roles")]
    public async Task<IActionResult> AssignRolesToUser(string id, [FromBody] AssignRolesRequest model)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        // Remover roles actuales
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Asignar nuevos roles
        var validRoles = new List<string>();
        foreach (var role in model.Roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                validRoles.Add(role);
            }
        }

        if (validRoles.Any())
        {
            var result = await _userManager.AddToRolesAsync(user, validRoles);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { message = "Roles asignados exitosamente", roles = validRoles });
    }

    // POST: api/usermanagement/users/{id}/lock
    [HttpPost("users/{id}/lock")]
    public async Task<IActionResult> LockUser(string id, [FromBody] LockUserModel model)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == id)
            return BadRequest(new { message = "No puedes bloquear tu propia cuenta" });

        DateTimeOffset? lockoutEnd = model.LockoutMinutes.HasValue
            ? DateTimeOffset.UtcNow.AddMinutes(model.LockoutMinutes.Value)
            : DateTimeOffset.MaxValue; // Bloqueo permanente

        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new
        {
            message = "Usuario bloqueado exitosamente",
            lockoutEnd = lockoutEnd
        });
    }

    // POST: api/usermanagement/users/{id}/unlock
    [HttpPost("users/{id}/unlock")]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Usuario desbloqueado exitosamente" });
    }

    // POST: api/usermanagement/users/{id}/reset-password
    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordModel model)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        // Remover contraseña actual y establecer nueva
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);

        if (!removePasswordResult.Succeeded)
            return BadRequest(new { errors = removePasswordResult.Errors.Select(e => e.Description) });

        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);

        if (!addPasswordResult.Succeeded)
            return BadRequest(new { errors = addPasswordResult.Errors.Select(e => e.Description) });

        return Ok(new { message = "Contraseña restablecida exitosamente" });
    }

    // GET: api/usermanagement/roles
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();

        return Ok(roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty
        }));
    }

    // GET: api/usermanagement/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var allUsers = await _userManager.Users.AsNoTracking().ToListAsync();
        var totalUsers = allUsers.Count;

        int adminCount = 0;
        int userCount = 0;
        int lockedCount = 0;

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) adminCount++;
            if (roles.Contains("User")) userCount++;

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                lockedCount++;
        }

        var statsDto = new UserStatsDto
        {
            TotalUsers = totalUsers,
            AdminCount = adminCount,
            UserCount = userCount,
            LockedUsers = lockedCount,
            ActiveUsers = totalUsers - lockedCount
        };
        return Ok(statsDto);
    }
}
