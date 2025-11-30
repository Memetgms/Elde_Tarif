using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _cfg;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration cfg,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cfg = cfg;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null)
                return BadRequest("Bu e-posta zaten kayıtlı.");

            var username = string.IsNullOrWhiteSpace(dto.UserName)
                ? dto.Email.Split('@')[0]
                : dto.UserName!.Trim();

            var user = new AppUser
            {
                Email = dto.Email.Trim(),
                UserName = username
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(x => x.Description));

            var token = CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                message = "Kayıt başarılı",
                token,
                refreshToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AppUser? user = dto.EmailOrUserName.Contains("@")
                ? await _userManager.FindByEmailAsync(dto.EmailOrUserName)
                : await _userManager.FindByNameAsync(dto.EmailOrUserName);

            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Unauthorized("Geçersiz şifre.");

            var token = CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                message = "Giriş başarılı",
                token,
                refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Refresh token gerekli.");

            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
            if (user == null)
                return Unauthorized("Geçersiz refresh token.");

            if (user.RefreshTokenExpireDate == null || user.RefreshTokenExpireDate < DateTime.UtcNow)
                return Unauthorized("Refresh token süresi dolmuş.");

            var newAccessToken = CreateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            user.RefreshToken = null;
            user.RefreshTokenExpireDate = null;
            await _userManager.UpdateAsync(user);

            return Ok("Çıkış yapıldı.");
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.Favoriler)
                .Include(u => u.Yorumlar)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Unauthorized();

            var ogunSayisi = await _context.GunlukOgunler
                .CountAsync(x => x.KullaniciId == userId);

            var dto = new ProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                AdSoyad = user.AdSoyad,

                FavoriSayisi = user.Favoriler.Count,
                YorumSayisi = user.Yorumlar.Count,
                OgunSayisi = ogunSayisi
            };

            return Ok(dto);
        }


        private string CreateJwtToken(AppUser user)
        {
            var jwt = _cfg.GetSection("Jwt");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"]!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        

    }
}