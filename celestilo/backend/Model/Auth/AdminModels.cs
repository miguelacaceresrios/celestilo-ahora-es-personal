using System.ComponentModel.DataAnnotations;
namespace backend.Model.Auth;

/// <summary>
/// Model representing a request to create a new user from the admin panel.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the username for the new user account.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the new user account.
    /// Must be a valid email address format.
    /// </summary>
    [Required]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the new user account.
    /// Must be at least 6 characters long.
    /// </summary>
    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the email address is confirmed.
    /// Defaults to false.
    /// </summary>
    public bool EmailConfirmed { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of roles to assign to the new user.
    /// If null or empty, the default "User" role will be assigned.
    /// </summary>
    public List<string>? Roles { get; set; }
}

/// <summary>
/// Model representing a request to update an existing user's information.
/// All properties are optional and only provided fields will be updated.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Gets or sets the new username. Null if not being updated.
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Gets or sets the new email address. Must be a valid email format if provided. Null if not being updated.
    /// </summary>
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }
    
    /// <summary>
    /// Gets or sets the email confirmation status. Null if not being updated.
    /// </summary>
    public bool? EmailConfirmed { get; set; }
    
    /// <summary>
    /// Gets or sets the phone number. Null if not being updated.
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Model representing a request to assign roles to a user.
/// Existing roles will be replaced with the roles specified in this request.
/// </summary>
public class AssignRolesRequest
{
    /// <summary>
    /// Gets or sets the list of role names to assign to the user.
    /// </summary>
    [Required]
    public List<string> Roles { get; set; } = new List<string>();
}

/// <summary>
/// Model representing a request to lock a user account.
/// </summary>
public class LockUserModel
{
    /// <summary>
    /// Gets or sets the number of minutes to lock the user account.
    /// If null, the account will be permanently locked.
    /// </summary>
    public int? LockoutMinutes { get; set; }
}

/// <summary>
/// Model representing a request to reset a user's password.
/// </summary>
public class ResetPasswordModel
{
    /// <summary>
    /// Gets or sets the new password for the user.
    /// Must be at least 6 characters long.
    /// </summary>
    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}
