using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GunlukController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GunlukController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // ----------------------------------------------------
        // 1) ÖĞÜNE TARİF EKLE
        // ----------------------------------------------------
        [HttpPost("ogun-ekle")]
        public async Task<IActionResult> OgunEkle([FromBody] GunlukOgunEkleDTO dto)
        {
            var userId = GetUserId();

            var ogun = new GunlukOgun
            {
                KullaniciId = userId,
                OgunTipi = dto.OgunTipi,
                TarifId = dto.TarifId,
                Tarih = DateTime.UtcNow.Date
            };

            _context.GunlukOgunler.Add(ogun);
            await _context.SaveChangesAsync();

            return Ok("Öğün eklendi.");
        }

        // ----------------------------------------------------
        // 2) GÜNLÜK ÖĞÜNLERİ GETİR
        // ----------------------------------------------------
        [HttpGet("ogunler")]
        public async Task<IActionResult> GetOgunler()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;


            var ogunler = await _context.GunlukOgunler
                .Include(o => o.Tarif)
                .Where(o => o.KullaniciId == userId && o.Tarih == today)
                .Select(o => new GunlukOgunGetDTO
                {
                    Id = o.Id,
                    OgunTipi = o.OgunTipi,
                    TarifId = o.TarifId,
                    TarifBaslik = o.Tarif.Baslik,
                    Kalori = o.Tarif.KaloriKcal ?? 0,
                    Protein = o.Tarif.ProteinGr ?? 0,
                    Karbonhidrat = o.Tarif.KarbonhidratGr ?? 0,
                    Yag = o.Tarif.YagGr ?? 0
                })
                .ToListAsync();

            return Ok(ogunler);
        }

        // ----------------------------------------------------
        // 3) ÖĞÜNDEN TARİF SİL
        // ----------------------------------------------------
        [HttpDelete("ogun-sil/{id}")]
        public async Task<IActionResult> OgunSil(int id)
        {
            var userId = GetUserId();
            var ogun = await _context.GunlukOgunler
                .FirstOrDefaultAsync(x => x.Id == id && x.KullaniciId == userId);

            if (ogun == null)
                return NotFound("Öğün bulunamadı.");

            _context.GunlukOgunler.Remove(ogun);
            await _context.SaveChangesAsync();

            return Ok("Silindi.");
        }

        // ----------------------------------------------------
        // 4) GÜNLÜK MAKROLARI GETİR
        // ----------------------------------------------------
        [HttpGet("makrolar")]
        public async Task<IActionResult> GetMakrolar()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;


            var ogunler = await _context.GunlukOgunler
                .Include(o => o.Tarif)
                .Where(o => o.KullaniciId == userId && o.Tarih == today)
                .ToListAsync();

            var toplam = new
            {
                Kalori = ogunler.Sum(o => o.Tarif.KaloriKcal ?? 0),
                Protein = ogunler.Sum(o => o.Tarif.ProteinGr ?? 0),
                Karbonhidrat = ogunler.Sum(o => o.Tarif.KarbonhidratGr ?? 0),
                Yag = ogunler.Sum(o => o.Tarif.YagGr ?? 0)
            };

            return Ok(toplam);
        }

        

    }
}
