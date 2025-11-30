using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class GunlukOgun
    {
        public int Id { get; set; }

        // Kullanıcı ID (Identity)
        public string KullaniciId { get; set; } = string.Empty;
        public AppUser Kullanici { get; set; } = null!;

        // Tarih (günlük plan için)
        public DateTime Tarih { get; set; } = DateTime.UtcNow.Date;

        // Öğün tipi
        [Required]
        public string OgunTipi { get; set; } = string.Empty;
        // Kahvalti, Oglen, Aksam, Ara

        // Tarif
        public int TarifId { get; set; }
        public Tarif Tarif { get; set; } = null!;
    }
}
