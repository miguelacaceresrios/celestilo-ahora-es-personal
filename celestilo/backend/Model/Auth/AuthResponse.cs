namespace backend.Model.Auth;

/// <summary>
/// Model representing the authentication response returned after successful login or registration.
/// Contains the JWT token and user information.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Gets or sets the JWT token used for authenticated API requests.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the username of the authenticated user.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email address of the authenticated user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the list of role names assigned to the authenticated user.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();
}
