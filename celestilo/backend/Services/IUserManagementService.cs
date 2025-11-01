using backend.DTOs;
using backend.Model.Auth;
namespace backend.Services;

/// <summary>
/// Interface defining user management service operations for managing users, roles, and user-related operations.
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Retrieves all users from the system.
    /// </summary>
    /// <returns>A collection of <see cref="UserDto"/> objects representing all users.</returns>
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    
    /// <summary>
    /// Retrieves a specific user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>
    /// The <see cref="UserDto"/> object if found, otherwise null.
    /// </returns>
    Task<UserDto?> GetUserByIdAsync(string id);
    
    /// <summary>
    /// Creates a new user account in the system.
    /// </summary>
    /// <param name="model">The user creation model containing username, email, password, and optional roles.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - The created user's ID if successful, otherwise null.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    Task<(bool Success, string? UserId, IEnumerable<string>? Errors)> CreateUserAsync(CreateUserRequest model);
    
    /// <summary>
    /// Updates an existing user's information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="model">The user update model containing the fields to update.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    Task<(bool Success, IEnumerable<string>? Errors)> UpdateUserAsync(string id, UpdateUserRequest model);
    
    /// <summary>
    /// Deletes a user account from the system.
    /// Prevents users from deleting their own account.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <param name="currentUserId">The unique identifier of the currently authenticated user.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    Task<(bool Success, IEnumerable<string>? Errors)> DeleteUserAsync(string id, string currentUserId);
    
    /// <summary>
    /// Assigns roles to a user. Existing roles are replaced with the new ones.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="roles">The collection of role names to assign to the user.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of successfully assigned role names.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    Task<(bool Success, IEnumerable<string> AssignedRoles, IEnumerable<string>? Errors)> AssignRolesToUserAsync(string id, IEnumerable<string> roles);
    
    /// <summary>
    /// Locks a user account, preventing login. Can be temporary or permanent.
    /// Prevents users from locking their own account.
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
    Task<(bool Success, DateTimeOffset? LockoutEnd, IEnumerable<string>? Errors)> LockUserAsync(string id, string currentUserId, int? lockoutMinutes);
    
    /// <summary>
    /// Unlocks a previously locked user account, allowing login again.
    /// </summary>
    /// <param name="id">The unique identifier of the user to unlock.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    Task<(bool Success, IEnumerable<string>? Errors)> UnlockUserAsync(string id);
    
    /// <summary>
    /// Resets a user's password to a new value.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose password should be reset.</param>
    /// <param name="newPassword">The new password for the user.</param>
    /// <returns>
    /// A tuple containing:
    /// - A boolean indicating if the operation succeeded.
    /// - A collection of error messages if the operation failed, otherwise null.
    /// </returns>
    Task<(bool Success, IEnumerable<string>? Errors)> ResetPasswordAsync(string id, string newPassword);
    
    /// <summary>
    /// Retrieves all available roles in the system.
    /// </summary>
    /// <returns>A collection of <see cref="RoleDto"/> objects representing all roles.</returns>
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    
    /// <summary>
    /// Retrieves user statistics including total users, admin count, user count, and lockout information.
    /// </summary>
    /// <returns>A <see cref="UserStatsDto"/> object containing user statistics.</returns>
    Task<UserStatsDto> GetUserStatsAsync();
}