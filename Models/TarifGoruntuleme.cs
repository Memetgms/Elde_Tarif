using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class TarifGoruntuleme
    {
        public int Id { get; set; }

        // Login varsa dolar, yoksa null kalır
        public string? KullaniciId { get; set; }
        public AppUser? Kullanici { get; set; }

        // Login yoksa cihaz bazlı takip için
        [MaxLength(80)]
        public string? AnonId { get; set; }

        public int TarifId { get; set; }
        public Tarif Tarif { get; set; } = null!;

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
