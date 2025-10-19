namespace Elde_Tarif.DTO
{
    public class TumSeflerDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
    }

    public class SefEkleDto
    {
        public string Ad { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}
