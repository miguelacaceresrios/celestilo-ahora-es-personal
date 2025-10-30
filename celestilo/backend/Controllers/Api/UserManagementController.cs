using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Model.Auth;
namespace backend.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserManagementController(IUserManagementService userManagementService, UserManager<IdentityUser> userManager) : ControllerBase
{
    // GET: api/usermanagement/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userManagementService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/usermanagement/users/{id}
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await userManagementService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(user);
    }

    // POST: api/usermanagement/users
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, userId, errors) = await userManagementService.CreateUserAsync(model);

        if (!success)
            return BadRequest(new { errors });

        return Ok(new { message = "Usuario creado exitosamente", userId });
    }

    // PUT: api/usermanagement/users/{id}
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, errors) = await userManagementService.UpdateUserAsync(id, model);

        if (!success)
            return BadRequest(new { errors });

        return Ok(new { message = "Usuario actualizado exitosamente" });
    }

    // DELETE: api/usermanagement/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = userManager.GetUserId(User);
        var (success, errors) = await userManagementService.DeleteUserAsync(id, currentUserId!);

        if (!success)
        {
            var errorMessage = errors?.FirstOrDefault() ?? "Error al eliminar usuario";
            if (errorMessage.Contains("propia cuenta"))
                return BadRequest(new { message = errorMessage });

            return NotFound(new { message = errorMessage });
        }

        return Ok(new { message = "Usuario eliminado exitosamente" });
    }

    // POST: api/usermanagement/users/{id}/roles
    [HttpPost("users/{id}/roles")]
    public async Task<IActionResult> AssignRolesToUser(string id, [FromBody] AssignRolesRequest model)
    {
        var (success, assignedRoles, errors) = await userManagementService.AssignRolesToUserAsync(id, model.Roles);

        if (!success)
        {
            var errorMessage = errors?.FirstOrDefault() ?? "Error al asignar roles";
            if (errorMessage.Contains("no encontrado"))
                return NotFound(new { message = errorMessage });

            return BadRequest(new { errors });
        }

        return Ok(new { message = "Roles asignados exitosamente", roles = assignedRoles });
    }

    // POST: api/usermanagement/users/{id}/lock
    [HttpPost("users/{id}/lock")]
    public async Task<IActionResult> LockUser(string id, [FromBody] LockUserModel model)
    {
        var currentUserId = userManager.GetUserId(User);
        var (success, lockoutEnd, errors) = await userManagementService.LockUserAsync(
            id,
            currentUserId!,
            model.LockoutMinutes);

        if (!success)
        {
            var errorMessage = errors?.FirstOrDefault() ?? "Error al bloquear usuario";
            if (errorMessage.Contains("propia cuenta"))
                return BadRequest(new { message = errorMessage });

            return NotFound(new { message = errorMessage });
        }

        return Ok(new
        {
            message = "Usuario bloqueado exitosamente",
            lockoutEnd
        });
    }

    // POST: api/usermanagement/users/{id}/unlock
    [HttpPost("users/{id}/unlock")]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var (success, errors) = await userManagementService.UnlockUserAsync(id);

        if (!success)
        {
            var errorMessage = errors?.FirstOrDefault() ?? "Error al desbloquear usuario";
            if (errorMessage.Contains("no encontrado"))
                return NotFound(new { message = errorMessage });

            return BadRequest(new { errors });
        }

        return Ok(new { message = "Usuario desbloqueado exitosamente" });
    }

    // POST: api/usermanagement/users/{id}/reset-password
    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordModel model)
    {
        var (success, errors) = await userManagementService.ResetPasswordAsync(id, model.NewPassword);

        if (!success)
        {
            var errorMessage = errors?.FirstOrDefault() ?? "Error al restablecer contraseña";
            if (errorMessage.Contains("no encontrado"))
                return NotFound(new { message = errorMessage });

            return BadRequest(new { errors });
        }

        return Ok(new { message = "Contraseña restablecida exitosamente" });
    }

    // GET: api/usermanagement/roles
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await userManagementService.GetAllRolesAsync();
        return Ok(roles);
    }

    // GET: api/usermanagement/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var stats = await userManagementService.GetUserStatsAsync();
        return Ok(stats);
    }
}