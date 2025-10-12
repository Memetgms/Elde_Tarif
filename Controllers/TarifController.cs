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

        // POST: api/Tarifler tarif ekleme
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

        // POST: api/tarifler/{tarifId}/malzemeler tarife malzeme ekleme
        [HttpPost("{tarifId:int}/malzemeler")]
        public async Task<IActionResult> AddMalzemeler(int tarifId, [FromBody] List<TarifMalzemesiCreateDto> items)
        {
            if (items is null || items.Count == 0)
                return BadRequest("En az bir malzeme gönderin.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // 1) Tarif var mı?
            var tarifVarMi = await _context.Tarifler.AnyAsync(t => t.Id == tarifId);
            if (!tarifVarMi)
                return NotFound($"ID'si {tarifId} olan bir tarif bulunamadı.");

            // 2) Request içi tekrarları temizle (aynı MalzemeId birden fazla gönderilmişse ilkini al)
            var uniqueById = items
                .GroupBy(x => x.MalzemeId)
                .Select(g => g.First())
                .ToList();

            var gonderilenIds = uniqueById.Select(x => x.MalzemeId).ToList();

            // 3) Tüm malzemeler mevcut mu?
            var mevcutMalzemeIds = await _context.Malzemeler
                .Where(m => gonderilenIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            var eksikIds = gonderilenIds.Except(mevcutMalzemeIds).ToList();
            if (eksikIds.Count > 0)
                return BadRequest($"Bulunamayan MalzemeId'ler: {string.Join(", ", eksikIds)}");

            // 4) Tarif içinde zaten olanları bul
            var tarifteVarOlanIds = await _context.TarifMalzemeleri
                .Where(tm => tm.TarifId == tarifId && gonderilenIds.Contains(tm.MalzemeId))
                .Select(tm => tm.MalzemeId)
                .ToListAsync();

            // 5) Eklenecekleri hazırla (var olanları atla)
            var eklenecekler = uniqueById
                .Where(x => !tarifteVarOlanIds.Contains(x.MalzemeId))
                .Select(x => new TarifMalzemesi
                {
                    TarifId = tarifId,
                    MalzemeId = x.MalzemeId,
                    Aciklama = string.IsNullOrWhiteSpace(x.Aciklama) ? null : x.Aciklama.Trim()
                })
                .ToList();

            // 6) Ekle ve kaydet
            if (eklenecekler.Count > 0)
            {
                await _context.TarifMalzemeleri.AddRangeAsync(eklenecekler);
                await _context.SaveChangesAsync();
            }

            var eklenen = eklenecekler.Count;
            var atlanan = uniqueById.Count - eklenen; // request içi tekrar + tarifte zaten olan

            // Sadece kısa bir mesaj dön
            return Ok($"Tarife malzeme ekleme tamamlandı. Eklenen: {eklenen}, Atlanan: {atlanan}.");
        }

        // POST: api/tarifler/{tarifId}/yapim-adimlari  ilgili tarife yapım adımları ekleme
        [HttpPost("{tarifId:int}/yapim-adimlari")]
        public async Task<IActionResult> AddYapimAdimlari(int tarifId, [FromBody] List<YapimAdimiEklemeDto> items)
        {
            if (items is null || items.Count == 0)
                return BadRequest("En az bir yapım adımı gönderin.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // 1) Tarif var mı?
            var tarif = await _context.Tarifler
                .Include(t => t.YapimAdimlari)
                .FirstOrDefaultAsync(t => t.Id == tarifId);

            if (tarif is null)
                return NotFound($"ID'si {tarifId} olan bir tarif bulunamadı.");

            // 2) Mevcut en büyük sıra
            int mevcutMaxSira = tarif.YapimAdimlari.Any()
                ? (tarif.YapimAdimlari.Max(a => a.Sira ?? 0))
                : 0;

            int next = mevcutMaxSira;

            // 3) Eklenecekleri hazırla (gelen sırayı aynen kullan; boşsa otomatik artır)
            var eklenecekler = new List<YapimAdimi>();
            foreach (var dto in items)
            {
                int sira = dto.Sira ?? (++next);

                eklenecekler.Add(new YapimAdimi
                {
                    TarifId = tarifId,
                    Sira = sira,
                    Aciklama = string.IsNullOrWhiteSpace(dto.Aciklama) ? null : dto.Aciklama.Trim()
                });
            }

            await _context.YapimAdimlari.AddRangeAsync(eklenecekler);
            await _context.SaveChangesAsync();

            return Ok($"Yapım adımları eklendi. Eklenen: {eklenecekler.Count}.");
        }

    }
}