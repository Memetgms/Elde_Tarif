using Elde_Tarif;
using Elde_Tarif.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Kullanıcısız erişim yok
    public class ActivityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ActivityController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) ACTIVITY LIST
        // GET: api/activity/my
        // ============================================================
        [HttpGet("my")]
        public async Task<IActionResult> MyActivity()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // FAVORİLER
            var favs = await _context.Favoriler
                .Where(f => f.KullaniciId == userId)
                .Select(f => new ActivityDTO
                {
                    Tarih = f.EklenmeTarihi,
                    Tip = "favori",
                    Mesaj = $"{f.Tarif.Baslik} tarifini favorilere ekledin",
                    TarifId = f.TarifId
                })
                .ToListAsync();

            // YORUMLAR
            var yorumlar = await _context.Yorumlar
                .Where(y => y.KullaniciId == userId)
                .Select(y => new ActivityDTO
                {
                    Tarih = y.OlusturulmaTarihi,
                    Tip = "yorum",
                    Mesaj = $"{y.Tarif.Baslik} tarifine yorum yaptın",
                    TarifId = y.TarifId
                })
                .ToListAsync();

            // ÖĞÜNLER
            var ogunler = await _context.GunlukOgunler
                .Where(g => g.KullaniciId == userId)
                .Select(g => new ActivityDTO
                {
                    Tarih = g.Tarih,
                    Tip = "ogun",
                    Mesaj = $"{g.Tarif.Baslik} tarifini günlüğe ekledin",
                    TarifId = g.TarifId
                })
                .ToListAsync();

            // RAM'DE BİRLEŞTİR (EF Core hatası ortadan kalkar)
            var result = favs
                .Concat(yorumlar)
                .Concat(ogunler)
                .OrderByDescending(x => x.Tarih)
                .ToList();

            return Ok(result);
        }
    }
}


