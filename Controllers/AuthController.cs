using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _cfg;
        private readonly IEmailSender _emailSender;

        public AuthController(UserManager<AppUser> userManager,
                              SignInManager<AppUser> signInManager,
                              IConfiguration cfg,
                              IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cfg = cfg;
            _emailSender = emailSender;
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
                UserName = username,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(x => x.Description));

            // 6 haneli güvenli kod
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.EmailActivationCode = code;
            user.EmailActivationExpiresAt = DateTime.UtcNow.AddMinutes(15);
            user.EmailActivationTryCount = 0;

            // Refresh token'ı register'da istemiyorsan burada hiç verme (önerilen)
            user.RefreshToken = null;
            user.RefreshTokenExpireDate = null;

            var upd = await _userManager.UpdateAsync(user);
            if (!upd.Succeeded)
                return StatusCode(500, "Kullanıcı güncellenemedi.");

            // Mail gönder (sende çalışan MailKit mantığını buraya koy)
            var html = $@"
        <div style='font-family:Arial;max-width:480px;margin:auto;border:1px solid #eee;border-radius:10px;padding:16px'>
            <h2 style='margin:0 0 12px 0'>Elde Tarif - Email Doğrulama</h2>
            <p>Aktivasyon kodun:</p>
            <div style='font-size:28px;font-weight:700;letter-spacing:6px;background:#f6f6f6;padding:12px;text-align:center;border-radius:8px'>
                {code}
            </div>
            <p style='margin-top:12px'>Bu kod <b>15 dakika</b> geçerlidir.</p>
        </div>";

            await _emailSender.SendAsync(user.Email!, "Email Doğrulama Kodunuz", html);

            return Ok(new
            {
                message = "Kayıt başarılı. Email doğrulama kodu gönderildi. Lütfen önce emailinizi doğrulayın."
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AppUser? user = dto.EmailOrUserName.Contains("@")
                ? await _userManager.FindByEmailAsync(dto.EmailOrUserName.Trim())
                : await _userManager.FindByNameAsync(dto.EmailOrUserName.Trim());

            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            //  Email doğrulaması zorunlu
            if (!user.EmailConfirmed)
                return Unauthorized("Email doğrulanmamış. Lütfen mailinize gelen kod ile doğrulayın.");

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

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
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

        [Authorize]
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

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCodeDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            if (user.EmailConfirmed) return Ok("Email zaten doğrulanmış.");

            if (user.EmailActivationExpiresAt == null || user.EmailActivationExpiresAt < DateTime.UtcNow)
                return BadRequest("Kodun süresi dolmuş. Yeni kod isteyin.");

            if (user.EmailActivationTryCount >= 5)
                return BadRequest("Çok fazla yanlış deneme. Yeni kod isteyin.");

            if (user.EmailActivationCode != dto.Code.Trim())
            {
                user.EmailActivationTryCount++;
                await _userManager.UpdateAsync(user);
                return BadRequest("Kod hatalı.");
            }

            user.EmailConfirmed = true;
            user.EmailActivationCode = null;
            user.EmailActivationExpiresAt = null;
            user.EmailActivationTryCount = 0;

            await _userManager.UpdateAsync(user);

            return Ok("Email başarıyla doğrulandı.");
        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] ResendCodeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email gerekli.");

            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

            // Kullanıcı yoksa bile bilgi sızdırmamak için OK dönebilirsin
            if (user == null)
                return Ok("Eğer kayıtlı bir hesabınız varsa doğrulama kodu gönderildi.");

            if (user.EmailConfirmed)
                return Ok("Email zaten doğrulanmış.");

            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.EmailActivationCode = code;
            user.EmailActivationExpiresAt = DateTime.UtcNow.AddMinutes(15);
            user.EmailActivationTryCount = 0;

            await _userManager.UpdateAsync(user);

            var html = $@"
        <div style='font-family:Arial;max-width:480px;margin:auto;border:1px solid #eee;border-radius:10px;padding:16px'>
            <h2 style='margin:0 0 12px 0'>Elde Tarif - Yeni Doğrulama Kodu</h2>
            <p>Yeni aktivasyon kodun:</p>
            <div style='font-size:28px;font-weight:700;letter-spacing:6px;background:#f6f6f6;padding:12px;text-align:center;border-radius:8px'>
                {code}
            </div>
            <p style='margin-top:12px'>Bu kod <b>15 dakika</b> geçerlidir.</p>
        </div>";

            await _emailSender.SendAsync(user.Email!, "Yeni Email Doğrulama Kodunuz", html);

            return Ok("Doğrulama kodu tekrar gönderildi.");
        }
    }
}