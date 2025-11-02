namespace Elde_Tarif.DTO
{
    public class TarifDetayDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? KapakFotoUrl { get; set; }
        public string? Aciklama { get; set; }

        public int KategoriId { get; set; }
        public string KategoriAd { get; set; } = string.Empty;

        public int? SefId { get; set; }
        public string? SefAd { get; set; }

        public int? HazirlikSuresiDakika { get; set; }
        public int? PismeSuresiDakika { get; set; }
        public int? PorsiyonSayisi { get; set; }

        public int? KaloriKcal { get; set; }
        public int? ProteinGr { get; set; }
        public int? KarbonhidratGr { get; set; }
        public int? YagGr { get; set; }

        public bool Yayinda { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
        public DateTime? GuncellenmeTarihi { get; set; }

        public int FavoriSayisi { get; set; }
        public bool? KullaniciFavoriMi { get; set; } // null: anonim kullanıcı

        public double? OrtalamaPuan { get; set; }
        public int YorumSayisi { get; set; }

        public List<DetayMalzemeDto> Malzemeler { get; set; } = new();
        public List<DetayYapimAdimiDto> YapimAdimlari { get; set; } = new();
        public List<DetayYorumDto> SonYorumlar { get; set; } = new(); // örnek: son 5
    }

    public class DetayMalzemeDto
    {
        public int MalzemeId { get; set; }
        public string MalzemeAd { get; set; } = string.Empty;
        public string? MalzemeTur { get; set; }
        public string? Aciklama { get; set; } // TarifMalzemesi.Aciklama
    }

    public class DetayYapimAdimiDto
    {
        public int Id { get; set; }
        public int? Sira { get; set; }
        public string? Aciklama { get; set; }
    }

    public class DetayYorumDto
    {
        public int Id { get; set; }
        public string KullaniciId { get; set; } = string.Empty;
        public string? KullaniciAdSoyad { get; set; }
        public string? Icerik { get; set; }
        public int? Puan { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
    }
}
