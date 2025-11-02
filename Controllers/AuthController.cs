using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _cfg;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration cfg)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cfg = cfg;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var username = string.IsNullOrWhiteSpace(dto.UserName)
                ? dto.Email.Split('@')[0]
                : dto.UserName.Trim();

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Bu e-posta adresi zaten kayıtlı.");

            var user = new AppUser
            {
                Email = dto.Email.Trim(),
                UserName = username
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            var token = CreateJwtToken(user);

            return Ok(new { message = "Kayıt başarılı", token });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            AppUser? user = dto.EmailOrUserName.Contains('@')
                ? await _userManager.FindByEmailAsync(dto.EmailOrUserName)
                : await _userManager.FindByNameAsync(dto.EmailOrUserName);

            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Unauthorized("Geçersiz parola.");

            var token = CreateJwtToken(user);

            return Ok(new { message = "Giriş başarılı", token });
        }

        
        private string CreateJwtToken(AppUser user)
        {
            

            var key = _cfg["Jwt:Key"]!;
            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var expireMinutes = int.TryParse(_cfg["Jwt:ExpireMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? "")
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );

            var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
