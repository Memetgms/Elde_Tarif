namespace Elde_Tarif.DTO
{
    public class ActivityDTO
    {
        public DateTime Tarih { get; set; }
        public string Tip { get; set; }   // favori | yorum | ogun
        public string Mesaj { get; set; }
        public int TarifId { get; set; }
    }
}