using System.Security.Claims;
using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YorumController : ControllerBase
    {
        private readonly AppDbContext _context;

        public YorumController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // 1) Tarifin yorumlarını listele (giriş gerekmez)
        // GET: api/yorum/tarif/5?skip=0&take=20
        // ------------------------------------------------------------
        [HttpGet("tarif/{tarifId:int}")]
        public async Task<IActionResult> GetByTarif(int tarifId, int skip = 0, int take = 20)
        {
            if (take <= 0) take = 20;
            if (take > 100) take = 100;
            if (skip < 0) skip = 0;

            var tarifVarMi = await _context.Tarifler
                .AsNoTracking()
                .AnyAsync(t => t.Id == tarifId);

            if (!tarifVarMi)
                return NotFound("Tarif bulunamadı.");

            var yorumlar = await _context.Yorumlar
                .AsNoTracking()
                .Where(y => y.TarifId == tarifId)
                .OrderByDescending(y => y.OlusturulmaTarihi)
                .Skip(skip)
                .Take(take)
                .Select(y => new YorumListItemDto
                {
                    Id = y.Id,
                    TarifId = y.TarifId,
                    KullaniciId = y.KullaniciId,
                    UserName = y.Kullanici.UserName ?? "",
                    Icerik = y.Icerik,
                    Puan = y.Puan,
                    OlusturulmaTarihi = y.OlusturulmaTarihi
                })
                .ToListAsync();

            return Ok(yorumlar);
        }

        // ------------------------------------------------------------
        // 2) Yorum ekle (giriş zorunlu)
        // POST: api/yorum
        // Body: { tarifId, icerik, puan }
        // ------------------------------------------------------------
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] YorumCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            // Tarif kontrol
            var tarifVarMi = await _context.Tarifler
                .AsNoTracking()
                .AnyAsync(t => t.Id == dto.TarifId);

            if (!tarifVarMi)
                return NotFound("Tarif bulunamadı.");

            var icerik = dto.Icerik?.Trim();

            // İçerik boş + puan yok => anlamsız
            if (string.IsNullOrWhiteSpace(icerik) && dto.Puan is null)
                return BadRequest("Yorum içeriği veya puan girilmelidir.");

            var yorum = new Yorum
            {
                TarifId = dto.TarifId,
                KullaniciId = userId,
                Icerik = string.IsNullOrWhiteSpace(icerik) ? null : icerik,
                Puan = dto.Puan,
                OlusturulmaTarihi = DateTime.UtcNow
            };

            _context.Yorumlar.Add(yorum);
            await _context.SaveChangesAsync();

            // frontend'e direkt basılacak DTO
            var created = await _context.Yorumlar
                .AsNoTracking()
                .Include(y => y.Kullanici)
                .Where(y => y.Id == yorum.Id)
                .Select(y => new YorumListItemDto
                {
                    Id = y.Id,
                    TarifId = y.TarifId,
                    KullaniciId = y.KullaniciId,
                    UserName = y.Kullanici.UserName ?? "",
                    Icerik = y.Icerik,
                    Puan = y.Puan,
                    OlusturulmaTarihi = y.OlusturulmaTarihi
                })
                .FirstAsync();

            return Ok(created);
        }

        // ------------------------------------------------------------
        // 3) Yorum güncelle (sadece yorum sahibi)
        // PUT: api/yorum/12
        // Body: { icerik, puan }
        // ------------------------------------------------------------
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] YorumUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var yorum = await _context.Yorumlar.FirstOrDefaultAsync(y => y.Id == id);
            if (yorum == null)
                return NotFound("Yorum bulunamadı.");

            if (yorum.KullaniciId != userId)
                return Forbid("Bu yorumu güncelleme yetkin yok.");

            // sadece gönderilen alanları güncelle
            if (dto.Icerik != null)
            {
                var icerik = dto.Icerik.Trim();
                yorum.Icerik = string.IsNullOrWhiteSpace(icerik) ? null : icerik;
            }

            if (dto.Puan.HasValue)
                yorum.Puan = dto.Puan;

            await _context.SaveChangesAsync();

            // updated dto dönelim
            var updated = await _context.Yorumlar
                .AsNoTracking()
                .Include(y => y.Kullanici)
                .Where(y => y.Id == id)
                .Select(y => new YorumListItemDto
                {
                    Id = y.Id,
                    TarifId = y.TarifId,
                    KullaniciId = y.KullaniciId,
                    UserName = y.Kullanici.UserName ?? "",
                    Icerik = y.Icerik,
                    Puan = y.Puan,
                    OlusturulmaTarihi = y.OlusturulmaTarihi
                })
                .FirstAsync();

            return Ok(updated);
        }

        // ------------------------------------------------------------
        // 4) Yorum sil (sadece yorum sahibi)
        // DELETE: api/yorum/12
        // ------------------------------------------------------------
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var yorum = await _context.Yorumlar.FirstOrDefaultAsync(y => y.Id == id);
            if (yorum == null)
                return NotFound("Yorum bulunamadı.");

            if (yorum.KullaniciId != userId)
                return Forbid("Bu yorumu silme yetkin yok.");

            _context.Yorumlar.Remove(yorum);
            await _context.SaveChangesAsync();

            return Ok("Yorum silindi.");
        }
    }
}
