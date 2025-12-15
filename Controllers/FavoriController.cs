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
    [Authorize] 
    public class FavoriController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriController(AppDbContext context)
        {
            _context = context;
        }

        
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
    }
}