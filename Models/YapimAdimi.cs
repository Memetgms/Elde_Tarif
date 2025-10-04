namespace Elde_Tarif.Models
{
    public class YapimAdimi
    {
        public int Id { get; set; }

        public int TarifId { get; set; }
        public Tarif Tarif { get; set; } = null!;

        public int? Sira { get; set; }

        public string? Aciklama { get; set; }
    }
}
