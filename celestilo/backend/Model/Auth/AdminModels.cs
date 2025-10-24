using System.ComponentModel.DataAnnotations;


    // Modelo para crear usuarios desde el panel admin
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; } = false;

        public List<string>? Roles { get; set; }
    }

    // Modelo para actualizar usuarios
    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }
        
        public bool? EmailConfirmed { get; set; }
        
        public string? PhoneNumber { get; set; }
    }

    // Modelo para asignar roles
    public class AssignRolesRequest
    {
        [Required]
        public List<string> Roles { get; set; } = new List<string>();
    }

    // Modelo para bloquear usuarios
    public class LockUserModel
    {
        public int? LockoutMinutes { get; set; } // null = permanente
    }

    // Modelo para restablecer contraseña
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
