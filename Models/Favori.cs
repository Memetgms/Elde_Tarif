namespace Elde_Tarif.Models
{
    public class Favori
    {
        public string KullaniciId { get; set; } = string.Empty;
        public AppUser Kullanici { get; set; } = null!;

        public int TarifId { get; set; }
        public Tarif Tarif { get; set; } = null!;

        public DateTime EklenmeTarihi { get; set; } = DateTime.UtcNow;
    }
}
