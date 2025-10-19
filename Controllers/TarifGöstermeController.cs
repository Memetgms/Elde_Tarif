using Elde_Tarif.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    public class TarifGöstermeController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TarifGöstermeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/tumtarifler")]
        public async Task<ActionResult<IEnumerable<TarifDetayDTO>>> GetTarifler()
        {
            var tarifler = await _context.Tarifler
                .Select(t => new TarifDetayDTO
                {
                    Id = t.Id,
                    Baslik = t.Baslik,
                    KategoriAd = t.Kategori != null ? t.Kategori.Ad : null,
                    SefAd = t.Sef != null ? t.Sef.Ad : null,
                    KapakFotoUrl = t.KapakFotoUrl,
                    Aciklama = t.Aciklama,
                    HazirlikSuresiDakika = t.HazirlikSuresiDakika,
                    PismeSuresiDakika = t.PismeSuresiDakika,
                    PorsiyonSayisi = t.PorsiyonSayisi,
                    KaloriKcal = t.KaloriKcal,
                    ProteinGr = t.ProteinGr,
                    KarbonhidratGr = t.KarbonhidratGr,
                    YagGr = t.YagGr
                })
                .ToListAsync();
            return (Ok(tarifler));

        }
        [HttpGet("api/tarifonizleme")]
        public async Task<ActionResult<IEnumerable<TarifOnizlemeDto>>> GetTarifOnizleme()
        {
            var tarifler = await _context.Tarifler
                .Select(t => new TarifOnizlemeDto
                {
                    Id = t.Id,
                    Baslik = t.Baslik,
                    KapakFotoUrl = t.KapakFotoUrl
                })
                .ToListAsync();
            return Ok(tarifler);
        }
    }
}
