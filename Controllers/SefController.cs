using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SefController : ControllerBase 
    {  
        private readonly AppDbContext _context;
        public SefController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // GET: api/Sef
        public async Task<ActionResult<IEnumerable<TumSeflerDto>>> TumSefleriGetir()
        {
            var sefler = await _context.Sefler
                .Select(s => new TumSeflerDto
                {
                    Id = s.Id,
                    Ad = s.Ad,
                    FotoUrl = s.FotoUrl ?? string.Empty
                })
                .ToListAsync();
            return Ok(sefler);
        }

        [HttpPost]
        public async Task<ActionResult> SefEkle(SefEkleDto sefEkleDto)
        {
            var yeniSef = new Sef
            {
                Ad = sefEkleDto.Ad,
                FotoUrl = sefEkleDto.FotoUrl,
                Aciklama = sefEkleDto.Aciklama
            };
            _context.Sefler.Add(yeniSef);
            await _context.SaveChangesAsync();
            return Ok("Şef başarıyla eklendi.");
        }

        // GET: api/sef/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSefWithTarifler(int id)
        {
            var sef = await _context.Sefler
                .Include(s => s.Tarifler)
                .ThenInclude(t => t.Kategori) // kategori bilgisi de dahil olsun
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Ad,
                    s.FotoUrl,
                    s.Aciklama,
                    Tarifler = s.Tarifler.Select(t => new
                    {
                        t.Id,
                        t.Baslik,
                        t.KapakFotoUrl,
                        t.KategoriId,
                        KategoriAdi = t.Kategori.Ad,
                        t.HazirlikSuresiDakika,
                        t.PorsiyonSayisi,
                    })
                })
                .FirstOrDefaultAsync();

            if (sef == null)
                return NotFound("Belirtilen ID'ye sahip bir şef bulunamadı.");

            return Ok(sef);
        }
    }
}
