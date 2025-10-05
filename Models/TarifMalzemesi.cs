using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class TarifMalzemesi
    {
        public int Id { get; set; }

        public int TarifId { get; set; }
        public Tarif Tarif { get; set; } = null!;

        public int MalzemeId { get; set; }
        public Malzeme Malzeme { get; set; } = null!;

        [MaxLength(500)]
        public string? Aciklama { get; set; }
    }
}
