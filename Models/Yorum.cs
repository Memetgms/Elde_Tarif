using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class Yorum
    {
        public int Id { get; set; }

        public int TarifId { get; set; }
        public Tarif Tarif { get; set; } = null!;

        // Identity string PK'ye karşılık
        [Required]
        public string KullaniciId { get; set; } = string.Empty;
        public AppUser Kullanici { get; set; } = null!;

        public string? Icerik { get; set; } // text

        public int? Puan { get; set; } 
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    }
}
