using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.DTO
{
    public class LoginDto
    {
        [Required, MaxLength(256)]
        public string EmailOrUserName { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? UserName { get; set; }

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

    }
}
