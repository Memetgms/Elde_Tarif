using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class Tarif
    {
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string Baslik { get; set; } = string.Empty;

        // FK'lar
        public int KategoriId { get; set; }
        public Kategori Kategori { get; set; } = null!;

        public int? SefId { get; set; }
        public Sef? Sef { get; set; }

        [MaxLength(500)]
        public string? KapakFotoUrl { get; set; }

        public string? Aciklama { get; set; } // text

        public int? HazirlikSuresiDakika { get; set; }
        public int? PismeSuresiDakika { get; set; }

        public int? PorsiyonSayisi { get; set; }

        public int? KaloriKcal { get; set; }
        public int? ProteinGr { get; set; }
        public int? KarbonhidratGr { get; set; }
        public int? YagGr { get; set; }

        public bool Yayinda { get; set; } = false;
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? GuncellenmeTarihi { get; set; }
        public int? ClusterId { get; set; }

        // Navigations
        public ICollection<YapimAdimi> YapimAdimlari { get; set; } = new List<YapimAdimi>();
        public ICollection<TarifMalzemesi> TarifMalzemeleri { get; set; } = new List<TarifMalzemesi>();
        public ICollection<Yorum> Yorumlar { get; set; } = new List<Yorum>();
        public ICollection<Favori> Favoriler { get; set; } = new List<Favori>();
        public ICollection<TarifGoruntuleme> Goruntulemeler { get; set; } = new List<TarifGoruntuleme>();

    }
}
