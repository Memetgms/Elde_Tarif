
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly GeminiChatService _chatService;

        public ChatController(GeminiChatService chatService)
        {
            _chatService = chatService;
        }

        // Yemek/mutfak filtresi için basit keyword listesi
        private static readonly string[] AllowedKeywords =
        {
            "tarif", "yemek", "malzeme", "pişirme", "pişir", "fırın", "tencere",
            "tava", "haşlama", "kızartma", "fırında", "porsiyon", "gram", "gr",
            "ml", "mutfak", "soğan", "sarımsak", "et", "tavuk", "balık",
            "makarna", "pilav", "çorba", "salata", "tatlı", "hamur işi",
            "baharat", "yağ", "zeytinyağı", "sos"
        };

        private bool IsFoodRelated(string message)
        {
            var lower = message.ToLower();
            return AllowedKeywords.Any(k => lower.Contains(k));
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> Post([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponseDto
                {
                    Reply = "Lütfen bir mesaj gönderin."
                });
            }

            // 1) Yemek/mutfak filtresi – alakalı değilse Gemini'ye gitme
            if (!IsFoodRelated(request.Message))
            {
                return Ok(new ChatResponseDto
                {
                    Reply = "Ben sadece yemek tarifleri, malzemeler ve mutfak konularında yardımcı olabilirim."
                });
            }

            try
            {
                // 2) Gemini'ye kullanıcı mesajını gönder
                var reply = await _chatService.GetChatCompletionAsync(request.Message);

                return Ok(new ChatResponseDto
                {
                    Reply = reply
                });
            }
            catch (Exception)
            {
                // Log ekleyebilirsin
                return StatusCode(500, new ChatResponseDto
                {
                    Reply = "Şu anda bir sorun yaşıyorum, lütfen daha sonra tekrar deneyin."
                });
            }
        }
    }
}