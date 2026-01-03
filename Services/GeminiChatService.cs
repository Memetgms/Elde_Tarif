// Services/GeminiChatService.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace YourNamespace.Services
{
    public class GeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _systemPrompt;

        public GeminiChatService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;

            _apiKey = config["Gemini:ApiKey"]
                      ?? throw new Exception("Gemini:ApiKey not configured.");

            _model = config["Gemini:Model"] ?? "gemini-2.0-flash-lite";

            // Sistem prompt (sadece yemek/mutfak asistanı)
            _systemPrompt = @"
Sen bir yemek tarifi asistanısın.

Görevin:
- Sadece yemek tarifleri, malzemeler, pişirme teknikleri, mutfak kültürü,
  porsiyon, pişirme süreleri, besin değerleri gibi mutfakla ilgili konularda cevap ver.
- Kullanıcıya net, adım adım tarifler, malzeme listeleri ve ipuçları sun.

Kurallar:
- Yemek/mutfak ile ilgisi olmayan sorulara cevap verme.
- Böyle bir soru gelirse, kibarca 'Ben sadece yemek tarifleri ve mutfak konularında yardımcı olabilirim.' diye yanıt ver.
- Sağlık/medikal tavsiye verme, 'uzmana danış' yönlendirmesi yap.
- Çok uzun paragraflar yerine, mümkün olduğunca madde madde veya adım adım anlat.
";
        }

        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            // Gemini Generative Language API endpoint
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                // Sistem prompt
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = _systemPrompt }
                    }
                },
                // Kullanıcı mesajı
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = userMessage }
                        }
                    }
                },
                // Üretim ayarları
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 400
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini error: {response.StatusCode} - {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // candidates[0].content.parts[0].text
            var candidates = root.GetProperty("candidates");
            var firstCandidate = candidates[0];
            var content = firstCandidate.GetProperty("content");
            var parts = content.GetProperty("parts");
            var text = parts[0].GetProperty("text").GetString();

            return text ?? string.Empty;
        }
    }
}