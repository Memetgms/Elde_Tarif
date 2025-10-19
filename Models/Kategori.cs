using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.Models
{
    public class Kategori
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Aciklama { get; set; }

        public int? Sira { get; set; }
        public string KategoriUrl{ get; set; }

        public ICollection<Tarif> Tarifler { get; set; } = new List<Tarif>();
    }
}
