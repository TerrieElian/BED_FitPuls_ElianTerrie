namespace FitPulse.Services;

public interface IEmailService
{
    Task SendInvoiceAsync(string toAddress, byte[] pdfBytes, string fileName);
}

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendInvoiceAsync(string toAddress, byte[] pdfBytes, string fileName)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = "Je FitPulse-factuur";

        var builder = new BodyBuilder
        {
            TextBody = "Bedankt voor je training! Je factuur vind je in bijlage."
        };
        builder.Attachments.Add(fileName, pdfBytes, new ContentType("application", "pdf"));
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}