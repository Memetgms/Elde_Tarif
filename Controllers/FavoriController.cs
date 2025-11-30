using Elde_Tarif;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // tüm favori işlemleri kullanıcıya özel
    public class FavoriController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) FAVORİYE EKLE
        // POST: api/favori/add/5
        // ============================================================
        [HttpPost("add/{tarifId}")]
        public async Task<IActionResult> AddFavorite(int tarifId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tarif var mı kontrol et
            var tarif = await _context.Tarifler.FirstOrDefaultAsync(t => t.Id == tarifId);
            if (tarif == null)
                return NotFound("Tarif bulunamadı.");

            // Zaten favoride mi?
            bool alreadyFav = await _context.Favoriler
                .AnyAsync(f => f.KullaniciId == userId && f.TarifId == tarifId);

            if (alreadyFav)
                return BadRequest("Bu tarif zaten favorilerde.");

            // Favori ekleme
            var fav = new Favori
            {
                KullaniciId = userId,
                TarifId = tarifId
            };

            await _context.Favoriler.AddAsync(fav);
            await _context.SaveChangesAsync();

            return Ok("Favorilere eklendi.");
        }

        // ============================================================
        // 2) FAVORİDEN ÇIKAR
        // DELETE: api/favori/remove/5
        // ============================================================
        [HttpDelete("remove/{tarifId}")]
        public async Task<IActionResult> RemoveFavorite(int tarifId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fav = await _context.Favoriler
                .FirstOrDefaultAsync(f => f.KullaniciId == userId && f.TarifId == tarifId);

            if (fav == null)
                return NotFound("Favoride böyle bir tarif yok.");

            _context.Favoriler.Remove(fav);
            await _context.SaveChangesAsync();

            return Ok("Favorilerden kaldırıldı.");
        }

        // ============================================================
        // 3) KULLANICININ TÜM FAVORİLERİ (SAVED SEKMESİ)
        // GET: api/favori/my
        // ============================================================
        [HttpGet("my")]
        public async Task<IActionResult> MyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favs = await _context.Favoriler
                .Where(f => f.KullaniciId == userId)
                .Select(f => new
                {
                    TarifId = f.Tarif.Id,
                    Baslik = f.Tarif.Baslik,
                    Resim = f.Tarif.KapakFotoUrl,
                    Sef = f.Tarif.Sef.Ad,
                    Kategori = f.Tarif.Kategori.Ad
                })
                .ToListAsync();

            return Ok(favs);
        }

        // ============================================================
        // 4) FAVORİDE Mİ? (Opsiyonel, frontend için faydalı)
        // GET: api/favori/check/5
        // ============================================================
        [HttpGet("check/{tarifId}")]
        public async Task<IActionResult> CheckFavorite(int tarifId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isFavorite = await _context.Favoriler
                .AnyAsync(f => f.KullaniciId == userId && f.TarifId == tarifId);

            return Ok(new { isFavorite });
        }
    }
}
