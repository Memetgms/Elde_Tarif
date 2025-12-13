using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    public MailKitEmailSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var s = _cfg.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(s["FromName"], s["FromEmail"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(s["Host"], int.Parse(s["Port"]!), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(s["User"], s["Pass"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
