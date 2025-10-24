public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool IsLockedOut { get; set; }
}

// DTO para representar la información de un rol
public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// DTO para las estadísticas de usuarios
public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int AdminCount { get; set; }
    public int UserCount { get; set; }
    public int LockedUsers { get; set; }
    public int ActiveUsers { get; set; }
}
