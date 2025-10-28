using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementService(UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
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
        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

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

    public async Task<(bool Success, string? UserId, IEnumerable<string>? Errors)> CreateUserAsync(CreateUserRequest model)
    {
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

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

        return (true, user.Id, null);
    }

    public async Task<(bool Success, IEnumerable<string>? Errors)> UpdateUserAsync(string id, UpdateUserRequest model)
    {
        var user = await _userManager.FindByIdAsync(id);
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

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        return (true, null);
    }

    public async Task<(bool Success, IEnumerable<string>? Errors)> DeleteUserAsync(string id, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        // No permitir eliminar al propio admin
        if (currentUserId == id)
            return (false, new[] { "No puedes eliminar tu propia cuenta" });

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        return (true, null);
    }

    public async Task<(bool Success, IEnumerable<string> AssignedRoles, IEnumerable<string>? Errors)> AssignRolesToUserAsync(
        string id, 
        IEnumerable<string> roles)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, Enumerable.Empty<string>(), new[] { "Usuario no encontrado" });

        // Remover roles actuales
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Asignar nuevos roles
        var validRoles = new List<string>();
        foreach (var role in roles)
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
                return (false, Enumerable.Empty<string>(), result.Errors.Select(e => e.Description));
        }

        return (true, validRoles, null);
    }

    public async Task<(bool Success, DateTimeOffset? LockoutEnd, IEnumerable<string>? Errors)> LockUserAsync(
        string id, 
        string currentUserId, 
        int? lockoutMinutes)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, null, new[] { "Usuario no encontrado" });

        if (currentUserId == id)
            return (false, null, new[] { "No puedes bloquear tu propia cuenta" });

        DateTimeOffset? lockoutEnd = lockoutMinutes.HasValue
            ? DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes.Value)
            : DateTimeOffset.MaxValue; // Bloqueo permanente

        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        return (true, lockoutEnd, null);
    }

    public async Task<(bool Success, IEnumerable<string>? Errors)> UnlockUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        return (true, null);
    }

    public async Task<(bool Success, IEnumerable<string>? Errors)> ResetPasswordAsync(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, new[] { "Usuario no encontrado" });

        // Remover contraseña actual y establecer nueva
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);

        if (!removePasswordResult.Succeeded)
            return (false, removePasswordResult.Errors.Select(e => e.Description));

        var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);

        if (!addPasswordResult.Succeeded)
            return (false, addPasswordResult.Errors.Select(e => e.Description));

        return (true, null);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty
        });
    }

    public async Task<UserStatsDto> GetUserStatsAsync()
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