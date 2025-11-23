namespace Elde_Tarif.DTO
{
    public class TarifDetayDTO
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? KategoriAd { get; set; }
        public string? SefAd { get; set; }
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

    public class TarifOnizlemeDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? KapakFotoUrl { get; set; }
        public int? PorsiyonSayisi { get; set; }
        public int? ToplamSure { get; set; }
        public int? SefId { get; set; }
        public int KategoriId { get; set; }
    }

}
