using Elde_Tarif.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

    }
}


