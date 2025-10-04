using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class Sef
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Ad { get; set; } = null!;

        [MaxLength(500)]
        public string? FotoUrl { get; set; }

        public string? Aciklama { get; set; } // text
        public ICollection<Tarif> Tarifler { get; set; } = new List<Tarif>();
    }
}
