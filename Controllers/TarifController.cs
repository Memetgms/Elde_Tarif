using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TariflerController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TariflerController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Tarifler
        [HttpPost]
        public async Task<IActionResult> CreateTarif([FromBody] TarifCreateDto tarifDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var kategoriExists = await _context.Kategoriler.AnyAsync(k => k.Id == tarifDto.KategoriId);
            if (!kategoriExists)
            {
                return BadRequest($"ID'si {tarifDto.KategoriId} olan bir kategori bulunamadı.");
            }

            if (tarifDto.SefId.HasValue)
            {
                var sefExists = await _context.Sefler.AnyAsync(s => s.Id == tarifDto.SefId.Value);
                if (!sefExists)
                {
                    return BadRequest($"ID'si {tarifDto.SefId.Value} olan bir şef bulunamadı.");
                }
            }

            var yeniTarif = new Tarif
            {
                // ... DTO'dan verileri Tarife aktarma ...
                Baslik = tarifDto.Baslik,
                KategoriId = tarifDto.KategoriId,
                SefId = tarifDto.SefId,
                KapakFotoUrl = tarifDto.KapakFotoUrl,
                Aciklama = tarifDto.Aciklama,
                HazirlikSuresiDakika = tarifDto.HazirlikSuresiDakika,
                PismeSuresiDakika = tarifDto.PismeSuresiDakika,
                PorsiyonSayisi = tarifDto.PorsiyonSayisi,
                KaloriKcal = tarifDto.KaloriKcal,
                ProteinGr = tarifDto.ProteinGr,
                KarbonhidratGr = tarifDto.KarbonhidratGr,
                YagGr = tarifDto.YagGr,
                Yayinda = false,
                OlusturulmaTarihi = DateTime.UtcNow
            };

            await _context.Tarifler.AddAsync(yeniTarif);
            await _context.SaveChangesAsync();

            
            return NoContent(); // HTTP 204
        }
    }
}