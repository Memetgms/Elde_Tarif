using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.DTO
{
    public class YapimAdimiEklemeDto
    {
        public int? Sira { get; set; }          // boş gelebilir, otomatik verilir
        [MaxLength(2000)]
        public string? Aciklama { get; set; }   // adım metni
    }
}
