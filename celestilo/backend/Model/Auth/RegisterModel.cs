using System.ComponentModel.DataAnnotations;
namespace backend.Model.Auth;

/// <summary>
/// Model representing user registration information.
/// Used for creating new user accounts in the system.
/// </summary>
public class RegisterModel
{
    /// <summary>
    /// Gets or sets the desired username for the new user account.
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the new user account.
    /// Must be a valid email address format.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the new user account.
    /// Must be at least 6 characters long.
    /// </summary>
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
