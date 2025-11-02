using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MalzemeController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MalzemeController(AppDbContext context)
        {
            _context = context;
        }

        // TOPLU MALZEME EKLEME
        // POST: api/malzeme
        [HttpPost]
        public async Task<IActionResult> EkleMalzemeler([FromBody] List<MalzemeCreateDTO> malzemeler)
        {
            if (malzemeler == null || malzemeler.Count == 0)
                return BadRequest("En az bir malzeme gönderin.");

            var yeniMalzemeler = malzemeler.Select(m => new Malzeme
            {
                Ad = m.Ad.Trim(),
                MalzemeUrl = m.MalzemeUrl,
                MalzemeTur = m.MalzemeTur,
                Aktif = m.Aktif
            }).ToList();

            await _context.Malzemeler.AddRangeAsync(yeniMalzemeler);
            await _context.SaveChangesAsync();

            return Ok("Malzemeler başarıyla eklendi.");
        }

        // MALZEME LİSTELEME api/malzeme
        [HttpGet]
        public async Task<IActionResult> GetMalzemeler()
        {
            var malzemeler = await _context.Malzemeler.Where(m => m.Aktif)
                .Select(m => new MalzemeGetDTO
                {
                    Id = m.Id,
                    Ad = m.Ad,
                    MalzemeTur = m.MalzemeTur,
                })
                .ToListAsync();
            return Ok(malzemeler);
        }
    }
}
