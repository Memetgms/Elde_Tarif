using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public UserController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager=userManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName
            });
        }
        [HttpGet("myactivity")]
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

            
            var result = favs
                .Concat(yorumlar)
                .Concat(ogunler)
                .OrderByDescending(x => x.Tarih)
                .ToList();

            return Ok(result);
        }

    }
}


