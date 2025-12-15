// Models/ChatDtos.cs
namespace YourNamespace.Models
{
    public class ChatRequestDTO
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseDTO
    {
        public string Reply { get; set; } = string.Empty;
    }
}