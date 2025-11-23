using Elde_Tarif.DTO;
using Elde_Tarif.Models;
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

        // TÜM TARİFLER (Detaylı liste)
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

            return Ok(tarifler);
        }

        // TARİF ÖNİZLEME (Sadece ID, Başlık, Foto, Süre, ŞefId, KategoriId)
        [HttpGet("api/tarifonizleme")]
        public async Task<ActionResult<IEnumerable<TarifOnizlemeDto>>> GetTarifOnizleme()
        {
            var tarifler = await _context.Tarifler
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

        // 🔹 TARİF DETAY (tek tarif)
        [HttpGet("api/tarif/{id}")]
        public async Task<ActionResult<TarifDetayDTO>> GetTarifDetay(int id)
        {
            var tarif = await _context.Tarifler
                .Include(t => t.Kategori)
                .Include(t => t.Sef)
                .Include(t => t.TarifMalzemeleri)
                    .ThenInclude(tm => tm.Malzeme)
                .Include(t => t.YapimAdimlari)
                .Include(t => t.Yorumlar)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarif == null)
                return NotFound($"ID'si {id} olan tarif bulunamadı.");

            var dto = new TarifDetayDto
            {
                Id = tarif.Id,
                Baslik = tarif.Baslik,
                KategoriAd = tarif.Kategori?.Ad,
                SefAd = tarif.Sef?.Ad,
                KapakFotoUrl = tarif.KapakFotoUrl,
                Aciklama = tarif.Aciklama,
                HazirlikSuresiDakika = tarif.HazirlikSuresiDakika,
                PismeSuresiDakika = tarif.PismeSuresiDakika,
                PorsiyonSayisi = tarif.PorsiyonSayisi,
                KaloriKcal = tarif.KaloriKcal,
                ProteinGr = tarif.ProteinGr,
                KarbonhidratGr = tarif.KarbonhidratGr,
                YagGr = tarif.YagGr,

                // Malzemeleri dön
                Malzemeler = tarif.TarifMalzemeleri
                    .Select(tm => new DetayMalzemeDto
                    {
                        MalzemeId = tm.MalzemeId,
                        MalzemeAd = tm.Malzeme.Ad,
                        MalzemeTur = tm.Malzeme.MalzemeTur,
                        Aciklama = tm.Aciklama
                    }).ToList(),

                // Yapım adımları
                YapimAdimlari = tarif.YapimAdimlari
                    .OrderBy(a => a.Sira ?? int.MaxValue)
                    .ThenBy(a => a.Id)
                    .Select(a => new DetayYapimAdimiDto
                    {
                        Id = a.Id,
                        Sira = a.Sira,
                        Aciklama = a.Aciklama
                    }).ToList(),

                // Yorumlar (ilk 5)
                SonYorumlar = tarif.Yorumlar
                    .OrderByDescending(y => y.OlusturulmaTarihi)
                    .Take(5)
                    .Select(y => new DetayYorumDto
                    {
                        Id = y.Id,
                        KullaniciId = y.KullaniciId,
                        KullaniciAdSoyad = y.Kullanici != null ? y.Kullanici.AdSoyad : null,
                        Icerik = y.Icerik,
                        Puan = y.Puan,
                        OlusturulmaTarihi = y.OlusturulmaTarihi
                    }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/tarifler/kategori/5
        [HttpGet("tarifbykategori/{id:int}")]
        public async Task<ActionResult<IEnumerable<TarifOnizlemeDto>>> GetTarifbyKategori(int id)
        {
            // Kategori var mı?
            var kategoriVarMi = await _context.Kategoriler.AnyAsync(k => k.Id == id);
            if (!kategoriVarMi)
                return NotFound($"ID'si {id} olan bir kategori bulunamadı.");

            var tarifler = await _context.Tarifler
                .Where(t => t.KategoriId == id)    
                .OrderByDescending(t => t.OlusturulmaTarihi)
                .Select(t => new TarifOnizlemeDto
                {
                    Id = t.Id,
                    Baslik = t.Baslik,
                    KapakFotoUrl = t.KapakFotoUrl,
                    ToplamSure=t.HazirlikSuresiDakika + t.PismeSuresiDakika
                })
                .ToListAsync();

            return Ok(tarifler);
        }
    }
}
