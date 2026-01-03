using System.ComponentModel.DataAnnotations;

namespace Elde_Tarif.DTO
{
    public class YorumCreateDto
    {
        [Required]
        public int TarifId { get; set; }

        [MaxLength(2000)]
        public string? Icerik { get; set; }

        [Range(1, 5)]
        public int? Puan { get; set; }
    }

    public class YorumUpdateDto
    {
        [MaxLength(2000)]
        public string? Icerik { get; set; }

        [Range(1, 5)]
        public int? Puan { get; set; }
    }

     
    public class YorumListItemDto
    {
        public int Id { get; set; }
        public int TarifId { get; set; }

        public string KullaniciId { get; set; } = string.Empty;

        // ⭐ SADECE USERNAME
        public string UserName { get; set; } = string.Empty;

        public string? Icerik { get; set; }
        public int? Puan { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
    }
}
