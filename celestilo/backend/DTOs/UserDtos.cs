namespace backend.DTOs;

/// <summary>
/// Data Transfer Object representing user information for API responses.
/// Used when returning user data to clients.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// Gets or sets the phone number of the user, if available.
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the list of role names assigned to the user.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the date and time when the user account lockout ends, if locked.
    /// Null if the account is not locked.
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the user account is currently locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }
}

/// <summary>
/// Data Transfer Object representing role information for API responses.
/// Used when returning role data to clients.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the role.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object representing user statistics.
/// Used when returning user statistics to clients.
/// </summary>
public class UserStatsDto
{
    /// <summary>
    /// Gets or sets the total number of users in the system.
    /// </summary>
    public int TotalUsers { get; set; }
    
    /// <summary>
    /// Gets or sets the number of users with the Admin role.
    /// </summary>
    public int AdminCount { get; set; }
    
    /// <summary>
    /// Gets or sets the number of users with the User role.
    /// </summary>
    public int UserCount { get; set; }
    
    /// <summary>
    /// Gets or sets the number of users who are currently locked out.
    /// </summary>
    public int LockedUsers { get; set; }
    
    /// <summary>
    /// Gets or sets the number of users who are currently active (not locked out).
    /// </summary>
    public int ActiveUsers { get; set; }
}
