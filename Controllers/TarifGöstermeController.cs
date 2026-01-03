using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TarifGostermeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TarifGostermeController(AppDbContext context)
        {
            _context = context;
        }

        //  TÜM TARİFLER (Detaylı liste)
        // GET: /api/TarifGosterme/tumtarifler
        [HttpGet("tumtarifler")]
        public async Task<ActionResult<IEnumerable<TarifDetayDTO>>> GetTarifler()
        {
            var tarifler = await _context.Tarifler
                .AsNoTracking()
                .OrderByDescending(t => t.OlusturulmaTarihi)
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

            return Ok(tarifler);
        }

        // TARİF ÖNİZLEME
        // GET: /api/TarifGosterme/tarifonizleme
        [HttpGet("tarifonizleme")]
        public async Task<ActionResult<IEnumerable<TarifOnizlemeDto>>> GetTarifOnizleme()
        {
            var tarifler = await _context.Tarifler
                .AsNoTracking()
                .OrderByDescending(t => t.OlusturulmaTarihi)
                .Select(t => new TarifOnizlemeDto
                {
                    Id = t.Id,
                    Baslik = t.Baslik,
                    KapakFotoUrl = t.KapakFotoUrl,
                    PorsiyonSayisi = t.PorsiyonSayisi,
                    ToplamSure = (t.HazirlikSuresiDakika ?? 0) + (t.PismeSuresiDakika ?? 0),
                    SefId = t.SefId,
                    KategoriId = t.KategoriId
                })
                .ToListAsync();

            return Ok(tarifler);
        }

        //  TARİF DETAY (tek tarif) - giriş yapmadan da görülebilir
        // GET: /api/TarifGosterme/tarif/5
        [HttpGet("tarif/{id:int}")]
        public async Task<ActionResult<TarifDetayDto>> GetTarifDetay(int id)
        {
            // kullanıcı giriş yaptıysa id al (anonimse null)
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var tarif = await _context.Tarifler
                .AsNoTracking()
                .Include(t => t.Kategori)
                .Include(t => t.Sef)
                .Include(t => t.TarifMalzemeleri)
                    .ThenInclude(tm => tm.Malzeme)
                .Include(t => t.YapimAdimlari)
                .Include(t => t.Yorumlar)
                    .ThenInclude(y => y.Kullanici)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarif == null)
                return NotFound($"ID'si {id} olan tarif bulunamadı.");

            // Yorum istatistikleri
            var yorumSayisi = tarif.Yorumlar.Count;
            double? ortalamaPuan = null;

            var puanli = tarif.Yorumlar.Where(y => y.Puan.HasValue).Select(y => y.Puan!.Value).ToList();
            if (puanli.Count > 0)
                ortalamaPuan = Math.Round(puanli.Average(), 2);

            // Favori sayısı (Favoriler tablosu varsa)
            // Not: Favori modelinde alan adları farklıysa burada düzelt.
            var favoriSayisi = await _context.Favoriler
                .AsNoTracking()
                .CountAsync(f => f.TarifId == id);

            bool? kullaniciFavoriMi = null;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                kullaniciFavoriMi = await _context.Favoriler
                    .AsNoTracking()
                    .AnyAsync(f => f.TarifId == id && f.KullaniciId == userId);
            }

            var dto = new TarifDetayDto
            {
                Id = tarif.Id,
                Baslik = tarif.Baslik,
                KapakFotoUrl = tarif.KapakFotoUrl,
                Aciklama = tarif.Aciklama,

                KategoriId = tarif.KategoriId,
                KategoriAd = tarif.Kategori?.Ad ?? string.Empty,

                SefId = tarif.SefId,
                SefAd = tarif.Sef?.Ad,

                HazirlikSuresiDakika = tarif.HazirlikSuresiDakika,
                PismeSuresiDakika = tarif.PismeSuresiDakika,
                PorsiyonSayisi = tarif.PorsiyonSayisi,

                KaloriKcal = tarif.KaloriKcal,
                ProteinGr = tarif.ProteinGr,
                KarbonhidratGr = tarif.KarbonhidratGr,
                YagGr = tarif.YagGr,

                Yayinda = tarif.Yayinda,
                OlusturulmaTarihi = tarif.OlusturulmaTarihi,
                GuncellenmeTarihi = tarif.GuncellenmeTarihi,

                FavoriSayisi = favoriSayisi,
                KullaniciFavoriMi = kullaniciFavoriMi,

                OrtalamaPuan = ortalamaPuan,
                YorumSayisi = yorumSayisi,

                // Malzemeler
                Malzemeler = tarif.TarifMalzemeleri
                    .Select(tm => new DetayMalzemeDto
                    {
                        MalzemeId = tm.MalzemeId,
                        MalzemeAd = tm.Malzeme.Ad,
                        MalzemeTur = tm.Malzeme.MalzemeTur,
                        Aciklama = tm.Aciklama
                    })
                    .ToList(),

                // Yapım adımları
                YapimAdimlari = tarif.YapimAdimlari
                    .OrderBy(a => a.Sira ?? int.MaxValue)
                    .ThenBy(a => a.Id)
                    .Select(a => new DetayYapimAdimiDto
                    {
                        Id = a.Id,
                        Sira = a.Sira,
                        Aciklama = a.Aciklama
                    })
                    .ToList(),

                // Son yorumlar  UserName ile
                SonYorumlar = tarif.Yorumlar
                    .OrderByDescending(y => y.OlusturulmaTarihi)
                    .Take(10)
                    .Select(y => new DetayYorumDto
                    {
                        Id = y.Id,
                        KullaniciId = y.KullaniciId,

                        // ✅ AppUser.UserName
                        UserName = y.Kullanici != null ? (y.Kullanici.UserName ?? "") : "",

                        Icerik = y.Icerik,
                        Puan = y.Puan,
                        OlusturulmaTarihi = y.OlusturulmaTarihi
                    })
                    .ToList()
            };

            return Ok(dto);
        }

        //  KATEGORİYE GÖRE TARİFLER (önizleme)
        // GET: /api/TarifGosterme/kategori/5
        [HttpGet("kategori/{id:int}")]
        public async Task<ActionResult<IEnumerable<TarifOnizlemeDto>>> GetTarifByKategori(int id)
        {
            var kategoriVarMi = await _context.Kategoriler.AsNoTracking().AnyAsync(k => k.Id == id);
            if (!kategoriVarMi)
                return NotFound($"ID'si {id} olan bir kategori bulunamadı.");

            var tarifler = await _context.Tarifler
                .AsNoTracking()
                .Where(t => t.KategoriId == id)
                .OrderByDescending(t => t.OlusturulmaTarihi)
                .Select(t => new TarifOnizlemeDto
                {
                    Id = t.Id,
                    Baslik = t.Baslik,
                    KapakFotoUrl = t.KapakFotoUrl,
                    PorsiyonSayisi = t.PorsiyonSayisi,
                    ToplamSure = (t.HazirlikSuresiDakika ?? 0) + (t.PismeSuresiDakika ?? 0),
                    SefId = t.SefId,
                    KategoriId = t.KategoriId
                })
                .ToListAsync();

            return Ok(tarifler);
        }

        // ✅ VIEW KAYDET (Authorize yok, guest için X-Client-Id header)
        // POST: /api/TarifGosterme/123/view
        // Header (guest): X-Client-Id: <guid>
        [HttpPost("{tarifId:int}/view")]
        public async Task<IActionResult> AddView(int tarifId)
        {
            // Tarif var mı kontrol et
            var tarifExists = await _context.Tarifler.AsNoTracking().AnyAsync(t => t.Id == tarifId);
            if (!tarifExists) return NotFound("Tarif bulunamadı.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // login varsa dolu olur
            var anonId = Request.Headers["X-Client-Id"].FirstOrDefault(); // guest için

            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(anonId))
                return Ok(new { recorded = false, reason = "no_user_or_anon" });

            var now = DateTime.UtcNow;
            var limit = now.AddMinutes(-5);

            var recentlyViewed = await _context.TarifGoruntulemeler
                .AsNoTracking()
                .AnyAsync(v =>
                    v.TarifId == tarifId &&
                    v.ViewedAt >= limit &&
                    (
                        (!string.IsNullOrWhiteSpace(userId) && v.KullaniciId == userId) ||
                        (string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(anonId) && v.AnonId == anonId)
                    )
                );

            if (recentlyViewed)
                return Ok(new { recorded = false, reason = "throttled_5min" });

            _context.TarifGoruntulemeler.Add(new TarifGoruntuleme
            {
                TarifId = tarifId,
                KullaniciId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                AnonId = string.IsNullOrWhiteSpace(userId) ? anonId : null,
                ViewedAt = now
            });

            await _context.SaveChangesAsync();
            return Ok(new { recorded = true });
        }

        // ✅ ANA SAYFA: Sana Özel 5 Öneri (son bakılan + favori -> cluster -> 5 tarif)
        // GET: /api/TarifGosterme/oneri/anasayfa?count=5
        // Header (guest): X-Client-Id: <guid>
        [HttpGet("oneri/anasayfa")]
        public async Task<ActionResult<IEnumerable<TarifOnizlemeDto>>> GetHomeRecommendations([FromQuery] int count = 5)
        {
            count = Math.Clamp(count, 1, 20);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var anonId = Request.Headers["X-Client-Id"].FirstOrDefault();

            // 1) Son 10 görüntüleme
            var viewQuery = _context.TarifGoruntulemeler.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(userId))
                viewQuery = viewQuery.Where(v => v.KullaniciId == userId);
            else if (!string.IsNullOrWhiteSpace(anonId))
                viewQuery = viewQuery.Where(v => v.AnonId == anonId);
            else
                viewQuery = viewQuery.Where(v => false);

            var lastViewedTarifIds = await viewQuery
                .OrderByDescending(v => v.ViewedAt)
                .Take(10)
                .Select(v => v.TarifId)
                .ToListAsync();

            // 2) Son 10 favori (sadece login kullanıcı)
            List<int> lastFavTarifIds = new();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                lastFavTarifIds = await _context.Favoriler
                    .AsNoTracking()
                    .Where(f => f.KullaniciId == userId)
                    .OrderByDescending(f => f.EklenmeTarihi)
                    .Take(10)
                    .Select(f => f.TarifId)
                    .ToListAsync();
            }

            var seedTarifIds = lastViewedTarifIds
                .Concat(lastFavTarifIds)
                .Distinct()
                .ToList();

            // Seed yoksa fallback: son eklenen tarifler
            if (seedTarifIds.Count == 0)
            {
                var fallback = await _context.Tarifler
                    .AsNoTracking()
                    .OrderByDescending(t => t.OlusturulmaTarihi)
                    .Take(count)
                    .Select(t => new TarifOnizlemeDto
                    {
                        Id = t.Id,
                        Baslik = t.Baslik,
                        KapakFotoUrl = t.KapakFotoUrl,
                        PorsiyonSayisi = t.PorsiyonSayisi,
                        ToplamSure = (t.HazirlikSuresiDakika ?? 0) + (t.PismeSuresiDakika ?? 0),
                        SefId = t.SefId,
                        KategoriId = t.KategoriId
                    })
                    .ToListAsync();

                return Ok(fallback);
            }

            // 3) Seed cluster’ları
            var seedClusters = await _context.Tarifler
                .AsNoTracking()
                .Where(t => seedTarifIds.Contains(t.Id) && t.ClusterId != null)
                .Select(t => t.ClusterId!.Value)
                .Distinct()
                .ToListAsync();

            // Cluster yoksa fallback: seed olmayan son tarifler
            if (seedClusters.Count == 0)
            {
                var fallback2 = await _context.Tarifler
                    .AsNoTracking()
                    .Where(t => !seedTarifIds.Contains(t.Id))
                    .OrderByDescending(t => t.OlusturulmaTarihi)
                    .Take(count)
                    .Select(t => new TarifOnizlemeDto
                    {
                        Id = t.Id,
                        Baslik = t.Baslik,
                        KapakFotoUrl = t.KapakFotoUrl,
                        PorsiyonSayisi = t.PorsiyonSayisi,
                        ToplamSure = (t.HazirlikSuresiDakika ?? 0) + (t.PismeSuresiDakika ?? 0),
                        SefId = t.SefId,
                        KategoriId = t.KategoriId
                    })
                    .ToListAsync();

                return Ok(fallback2);
            }

            // 4) Kullanıcının tüm favorileri (login için) -> tekrar önermeyelim
            List<int> allFavIds = new();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                allFavIds = await _context.Favoriler
                    .AsNoTracking()
                    .Where(f => f.KullaniciId == userId)
                    .Select(f => f.TarifId)
                    .ToListAsync();
            }

            // 5) Aynı cluster’dan adaylar
            var result = await _context.Tarifler
                .AsNoTracking()
                .Where(t => t.ClusterId != null
                            && seedClusters.Contains(t.ClusterId.Value)
                            && !seedTarifIds.Contains(t.Id)
                            && (string.IsNullOrWhiteSpace(userId) || !allFavIds.Contains(t.Id)))
                .OrderByDescending(t => t.OlusturulmaTarihi)
                .Take(count)
                .Select(t => new TarifOnizlemeDto
                {
                    Id = t.Id,
                    Baslik = t.Baslik,
                    KapakFotoUrl = t.KapakFotoUrl,
                    PorsiyonSayisi = t.PorsiyonSayisi,
                    ToplamSure = (t.HazirlikSuresiDakika ?? 0) + (t.PismeSuresiDakika ?? 0),
                    SefId = t.SefId,
                    KategoriId = t.KategoriId
                })
                .ToListAsync();

            // 6) Eksik kalırsa son tariflerle tamamla
            if (result.Count < count)
            {
                var need = count - result.Count;
                var exclude = result.Select(x => x.Id).Concat(seedTarifIds).ToHashSet();

                var fill = await _context.Tarifler
                    .AsNoTracking()
                    .Where(t => !exclude.Contains(t.Id))
                    .OrderByDescending(t => t.OlusturulmaTarihi)
                    .Take(need)
                    .Select(t => new TarifOnizlemeDto
                    {
                        Id = t.Id,
                        Baslik = t.Baslik,
                        KapakFotoUrl = t.KapakFotoUrl,
                        PorsiyonSayisi = t.PorsiyonSayisi,
                        ToplamSure = (t.HazirlikSuresiDakika ?? 0) + (t.PismeSuresiDakika ?? 0),
                        SefId = t.SefId,
                        KategoriId = t.KategoriId
                    })
                    .ToListAsync();

                result.AddRange(fill);
            }

            return Ok(result);
        }
    }
}
