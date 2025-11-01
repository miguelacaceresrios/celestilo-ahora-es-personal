using System.ComponentModel.DataAnnotations;
namespace backend.Model.Auth;

/// <summary>
/// Model representing user login credentials.
/// Used for authenticating users in the system.
/// </summary>
public class LoginModel
{
    /// <summary>
    /// Gets or sets the user's email address used for authentication.
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password for authentication.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
