using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class Malzeme
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? MalzemeUrl { get; set; }

        [MaxLength(100)]
        public string? MalzemeTur { get; set; }

        public bool Aktif { get; set; } = true;

        public ICollection<TarifMalzemesi> TarifMalzemeleri { get; set; } = new List<TarifMalzemesi>();
    }
}
