using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.DTO
{
    public class TarifEklemeDto
    {
        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [MaxLength(250)]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori ID zorunludur.")]
        public int KategoriId { get; set; }

        public int? SefId { get; set; }

        [MaxLength(500)]
        public string? KapakFotoUrl { get; set; }

        public string? Aciklama { get; set; }

        public int? HazirlikSuresiDakika { get; set; }
        public int? PismeSuresiDakika { get; set; }
        public int? PorsiyonSayisi { get; set; }

        
        public int? KaloriKcal { get; set; }
        public int? ProteinGr { get; set; }
        public int? KarbonhidratGr { get; set; }
        public int? YagGr { get; set; }
    }
    public class TarifMalzemesiEklemeDto
    {
        [Required]
        public int MalzemeId { get; set; }

        [MaxLength(200)]
        public string? Aciklama { get; set; }

    }
    public class TarifYapimAdimiEklemeDto
    {
        public int? Sira { get; set; }          // boş gelebilir, otomatik verilir
        [MaxLength(2000)]
        public string? Aciklama { get; set; }   // adım metni
    }
}
