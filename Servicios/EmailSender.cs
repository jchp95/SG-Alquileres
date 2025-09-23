using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Alquileres.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
            message.To.Add(new MailAddress(email));
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            client.EnableSsl = _emailSettings.EnableSsl;

            // Configuración adicional para evitar timeout
            client.Timeout = 30000; // 30 segundos
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            _logger.LogInformation($"Enviando email a {email} con asunto: {subject}");
            await client.SendMailAsync(message);
            _logger.LogInformation($"Email enviado correctamente a {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar email a {email}");
            throw; // Puedes manejar esto de forma diferente si prefieres no propagar la excepción
        }
    }
}