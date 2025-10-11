using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.DTO
{
    public class MalzemeCreateDTO
    {
        [Required, MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? MalzemeUrl { get; set; }

        [MaxLength(100)]
        public string? MalzemeTur { get; set; }

        public bool Aktif { get; set; } = true;
    }
}
