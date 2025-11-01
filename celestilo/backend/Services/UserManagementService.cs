using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;
using backend.Model.Auth;
namespace backend.Services;

/// <summary>
/// Service implementation for managing user accounts, roles, and user-related operations.
/// Provides comprehensive user management functionality including CRUD operations, role assignment,
/// account locking/unlocking, and password management.
/// </summary>
public class UserManagementService(UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager) : IUserManagementService
{
    /// <summary>
    /// Retrieves all users from the system with their associated roles and lockout status.
    /// Uses AsNoTracking() for read-only query optimization.
    /// </summary>
    /// <returns>A collection of <see cref="UserDto"/> objects representing all users.</returns>
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
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
        return userDtos;
    }

    /// <summary>
    /// Retrieves a specific user by their unique identifier with their associated roles and lockout status.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>
    /// The <see cref="UserDto"/> object if found, otherwise null.
    /// </returns>
    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return null;

        var roles = await userManager.GetRolesAsync(user);

        return new UserDto
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
    }

    /// <summary>
    /// Creates a new user account in the system.
    /// Assigns specified roles if provided, otherwise assigns the default "User" role.
    /// </summary>
    /// <param name="model">The user creation model containing username, email, password, and optional roles.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - The created user's ID if successful, otherwise null.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, string? UserId, IEnumerable<string>? Errors)> CreateUserAsync(CreateUserRequest model)
    {
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed
        };

        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        // Asignar roles si se especificaron
        if (model.Roles != null && model.Roles.Any())
        {
            foreach (var role in model.Roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
        else
        {
            // Rol por defecto
            await userManager.AddToRoleAsync(user, "User");
        }

        return (true, user.Id, null);
    }

    /// <summary>
    /// Updates an existing user's information.
    /// Only updates fields that are provided in the model (non-null values).
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="model">The user update model containing the fields to update.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, IEnumerable<string>? Errors)> UpdateUserAsync(string id, UpdateUserRequest model)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        // Actualizar información básica
        if (!string.IsNullOrEmpty(model.Username))
            user.UserName = model.Username;

        if (!string.IsNullOrEmpty(model.Email))
            user.Email = model.Email;

        if (model.EmailConfirmed.HasValue)
            user.EmailConfirmed = model.EmailConfirmed.Value;

        if (!string.IsNullOrEmpty(model.PhoneNumber))
            user.PhoneNumber = model.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        return (true, null);
    }

    /// <summary>
    /// Deletes a user account from the system.
    /// Prevents users from deleting their own account for security reasons.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <param name="currentUserId">The unique identifier of the currently authenticated user.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, IEnumerable<string>? Errors)> DeleteUserAsync(string id, string currentUserId)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        // No permitir eliminar al propio admin
        if (currentUserId == id)
            return (false, new[] { "No puedes eliminar tu propia cuenta" });

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        return (true, null);
    }

    /// <summary>
    /// Assigns roles to a user. Replaces all existing roles with the new ones.
    /// Only assigns roles that exist in the system.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="roles">The collection of role names to assign to the user.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of successfully assigned role names.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, IEnumerable<string> AssignedRoles, IEnumerable<string>? Errors)> AssignRolesToUserAsync(
        string id, 
        IEnumerable<string> roles)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return (false, Enumerable.Empty<string>(), new[] { "Usuario no encontrado" });

        // Remover roles actuales
        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);

        // Asignar nuevos roles
        var validRoles = new List<string>();
        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                validRoles.Add(role);
            }
        }

        if (validRoles.Any())
        {
            var result = await userManager.AddToRolesAsync(user, validRoles);

            if (!result.Succeeded)
                return (false, Enumerable.Empty<string>(), result.Errors.Select(e => e.Description));
        }

        return (true, validRoles, null);
    }

    /// <summary>
    /// Locks a user account, preventing login. Supports both temporary and permanent lockouts.
    /// Prevents users from locking their own account for security reasons.
    /// </summary>
    /// <param name="id">The unique identifier of the user to lock.</param>
    /// <param name="currentUserId">The unique identifier of the currently authenticated user.</param>
    /// <param name="lockoutMinutes">The number of minutes to lock the user (null for permanent lock).</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - The lockout end date if successful, otherwise null.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, DateTimeOffset? LockoutEnd, IEnumerable<string>? Errors)> LockUserAsync(
        string id, 
        string currentUserId, 
        int? lockoutMinutes)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return (false, null, new[] { "Usuario no encontrado" });

        if (currentUserId == id)
            return (false, null, new[] { "No puedes bloquear tu propia cuenta" });

        DateTimeOffset? lockoutEnd = lockoutMinutes.HasValue
            ? DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes.Value)
            : DateTimeOffset.MaxValue; // Bloqueo permanente

        var result = await userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        return (true, lockoutEnd, null);
    }

    /// <summary>
    /// Unlocks a previously locked user account, allowing login again.
    /// </summary>
    /// <param name="id">The unique identifier of the user to unlock.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, IEnumerable<string>? Errors)> UnlockUserAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        var result = await userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        return (true, null);
    }

    /// <summary>
    /// Resets a user's password to a new value.
    /// Removes the old password and sets the new one.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose password should be reset.</param>
    /// <param name="newPassword">The new password for the user.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    public async Task<(bool Success, IEnumerable<string>? Errors)> ResetPasswordAsync(string id, string newPassword)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        // Remover contraseña actual y establecer nueva
        var removePasswordResult = await userManager.RemovePasswordAsync(user);

        if (!removePasswordResult.Succeeded)
            return (false, removePasswordResult.Errors.Select(e => e.Description));

        var addPasswordResult = await userManager.AddPasswordAsync(user, newPassword);

        if (!addPasswordResult.Succeeded)
            return (false, addPasswordResult.Errors.Select(e => e.Description));

        return (true, null);
    }

    /// <summary>
    /// Retrieves all available roles in the system.
    /// Uses AsNoTracking() for read-only query optimization.
    /// </summary>
    /// <returns>A collection of <see cref="RoleDto"/> objects representing all roles.</returns>
    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await roleManager.Roles.AsNoTracking().ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty
        });
    }

    /// <summary>
    /// Retrieves user statistics including total users, admin count, user count, and lockout information.
    /// Uses AsNoTracking() for read-only query optimization.
    /// </summary>
    /// <returns>A <see cref="UserStatsDto"/> object containing user statistics.</returns>
    public async Task<UserStatsDto> GetUserStatsAsync()
    {
        var allUsers = await userManager.Users.AsNoTracking().ToListAsync();
        var totalUsers = allUsers.Count;

        int adminCount = 0;
        int userCount = 0;
        int lockedCount = 0;

        foreach (var user in allUsers)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) adminCount++;
            if (roles.Contains("User")) userCount++;

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                lockedCount++;
        }

        return new UserStatsDto
        {
            TotalUsers = totalUsers,
            AdminCount = adminCount,
            UserCount = userCount,
            LockedUsers = lockedCount,
            ActiveUsers = totalUsers - lockedCount
        };
    }
}