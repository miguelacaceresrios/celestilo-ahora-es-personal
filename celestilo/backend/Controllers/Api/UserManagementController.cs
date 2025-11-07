using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using backend.Services.Interfaces;
using backend.Model.Auth;
namespace backend.Controllers.Api;

/// <summary>
/// Controller responsible for managing user accounts, roles, and user-related operations.
/// All endpoints require Admin role authorization.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserManagementController(IUserManagementService userManagementService, UserManager<IdentityUser> userManager) : ControllerBase
{
    /// <summary>
    /// Retrieves all users from the system.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 200 OK containing a collection of <see cref="UserDto"/> objects.
    /// </returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userManagementService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a specific user by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 404 NotFound if the user with the specified ID does not exist.
    /// - 200 OK containing the <see cref="UserDto"/> object if found.
    /// </returns>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await userManagementService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(user);
    }

    /// <summary>
    /// Creates a new user account in the system.
    /// </summary>
    /// <param name="model">The user creation model containing username, email, password, and optional roles.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if the model state is invalid or user creation fails.
    /// - 200 OK with a success message and the created user ID upon successful creation.
    /// </returns>
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

    /// <summary>
    /// Updates an existing user's information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="model">The user update model containing the fields to update.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if the model state is invalid or update fails.
    /// - 200 OK with a success message upon successful update.
    /// </returns>
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

    /// <summary>
    /// Deletes a user account from the system.
    /// Prevents users from deleting their own account.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if attempting to delete own account.
    /// - 404 NotFound if the user with the specified ID does not exist.
    /// - 200 OK with a success message upon successful deletion.
    /// </returns>
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

    /// <summary>
    /// Assigns roles to a user. Existing roles are replaced with the new ones.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="model">The role assignment model containing the list of roles to assign.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 404 NotFound if the user with the specified ID does not exist.
    /// - 400 BadRequest if role assignment fails.
    /// - 200 OK with a success message and the assigned roles upon successful assignment.
    /// </returns>
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

    /// <summary>
    /// Locks a user account, preventing login. Can be temporary (specified minutes) or permanent (null).
    /// Prevents users from locking their own account.
    /// </summary>
    /// <param name="id">The unique identifier of the user to lock.</param>
    /// <param name="model">The lock model containing the lockout duration in minutes (null for permanent lock).</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if attempting to lock own account.
    /// - 404 NotFound if the user with the specified ID does not exist.
    /// - 200 OK with a success message and lockout end date upon successful lock.
    /// </returns>
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

    /// <summary>
    /// Unlocks a previously locked user account, allowing login again.
    /// </summary>
    /// <param name="id">The unique identifier of the user to unlock.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 404 NotFound if the user with the specified ID does not exist.
    /// - 400 BadRequest if unlock operation fails.
    /// - 200 OK with a success message upon successful unlock.
    /// </returns>
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

    /// <summary>
    /// Resets a user's password to a new value.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose password should be reset.</param>
    /// <param name="model">The password reset model containing the new password.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 404 NotFound if the user with the specified ID does not exist.
    /// - 400 BadRequest if password reset fails.
    /// - 200 OK with a success message upon successful password reset.
    /// </returns>
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

    /// <summary>
    /// Retrieves all available roles in the system.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 200 OK containing a collection of <see cref="RoleDto"/> objects.
    /// </returns>
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await userManagementService.GetAllRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Retrieves user statistics including total users, admin count, user count, and lockout information.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 200 OK containing a <see cref="UserStatsDto"/> object with user statistics.
    /// </returns>
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var stats = await userManagementService.GetUserStatsAsync();
        return Ok(stats);
    }
}