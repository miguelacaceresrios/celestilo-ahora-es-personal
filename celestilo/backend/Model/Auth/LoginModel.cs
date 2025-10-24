using System.ComponentModel.DataAnnotations;


    public class LoginModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
