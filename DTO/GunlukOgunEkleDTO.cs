namespace Elde_Tarif.DTO
{
    public class GunlukOgunEkleDTO
    {
        public string OgunTipi { get; set; } = string.Empty; // Kahvalti, Oglen, Aksam, Ara
        public int TarifId { get; set; }
    }

    public class GunlukOgunGetDTO
    {
        public int Id { get; set; }
        public string OgunTipi { get; set; } = string.Empty;
        public int TarifId { get; set; }
        public string TarifBaslik { get; set; } = string.Empty;
        public int Kalori { get; set; }
        public int Protein { get; set; }
        public int Karbonhidrat { get; set; }
        public int Yag { get; set; }
    }
}