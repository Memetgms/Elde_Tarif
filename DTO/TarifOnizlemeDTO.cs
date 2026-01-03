public class TarifOnizlemeDto
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public string? KapakFotoUrl { get; set; }
    public int? PorsiyonSayisi { get; set; }
    public int ToplamSure { get; set; }
    public int? SefId { get; set; }
    public int KategoriId { get; set; }
}