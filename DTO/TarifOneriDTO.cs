using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.DTO
{
    // 🔹 Kullanıcıdan gelen istek DTO'su
    public class TarifOneriIstekDto
    {
        [Required]
        public List<int> MalzemeIdler { get; set; } = new();

        // Örn: sadece %40 ve üzeri olanları getir
        public double? MinimumSkorYuzde { get; set; }

        // En az kaç malzeme eşleşsin? (default: 1)
        public int? MinimumEslesenMalzemeSayisi { get; set; }

        // Maksimum kaç tarif dönsün (default: 20)
        public int? MaksimumSonuc { get; set; }

        // 🔹 İstersen sadece en iyi cluster'dan öner (ML kullanımı)
        public bool SadeceEnIyiClusterdanMi { get; set; } = false;
    }

    // 🔹 API'nin döneceği sonuç DTO'su
    public class TarifOneriSonucDto
    {
        public int TarifId { get; set; }
        public string Baslik { get; set; } = string.Empty;

        // 0–100 arası skor
        public double SkorYuzde { get; set; }

        public double EslesenAgirlik { get; set; }
        public double ToplamAgirlik { get; set; }

        public int EslesenMalzemeSayisi { get; set; }
        public int TarifToplamMalzemeSayisi { get; set; }

        public int? ClusterId { get; set; }
        public string? TarifFoto { get; set; } 

        // Kullanıcıya göstermek için eşleşen malzeme isimleri
        public List<string> EslesenMalzemeler { get; set; } = new();
    }
}
