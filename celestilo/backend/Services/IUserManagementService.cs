public interface IUserManagementService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<(bool Success, string? UserId, IEnumerable<string>? Errors)> CreateUserAsync(CreateUserRequest model);
    Task<(bool Success, IEnumerable<string>? Errors)> UpdateUserAsync(string id, UpdateUserRequest model);
    Task<(bool Success, IEnumerable<string>? Errors)> DeleteUserAsync(string id, string currentUserId);
    Task<(bool Success, IEnumerable<string> AssignedRoles, IEnumerable<string>? Errors)> AssignRolesToUserAsync(string id, IEnumerable<string> roles);
    Task<(bool Success, DateTimeOffset? LockoutEnd, IEnumerable<string>? Errors)> LockUserAsync(string id, string currentUserId, int? lockoutMinutes);
    Task<(bool Success, IEnumerable<string>? Errors)> UnlockUserAsync(string id);
    Task<(bool Success, IEnumerable<string>? Errors)> ResetPasswordAsync(string id, string newPassword);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<UserStatsDto> GetUserStatsAsync();
}