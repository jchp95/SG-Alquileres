using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Alquileres.Services
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Extraer el enlace del mensaje HTML
            var linkMatch = Regex.Match(htmlMessage, @"href=['""](.+?)['""]");
            var resetLink = linkMatch.Success ? linkMatch.Groups[1].Value : "No se encontró enlace";

            // Decodificar el enlace para mejor legibilidad
            var decodedLink = Uri.UnescapeDataString(resetLink);

            // Formato mejorado para la consola
            var consoleOutput = new StringBuilder()
                .AppendLine("==================== 📧 CORREO SIMULADO ====================")
                .AppendLine($"Para: {email}")
                .AppendLine($"Asunto: {subject}")
                .AppendLine($"Enlace directo: {decodedLink}")
                .AppendLine("------------------------------------------------------------")
                .AppendLine("Contenido HTML:")
                .AppendLine(htmlMessage)
                .AppendLine("============================================================")
                .ToString();

            _logger.LogInformation(consoleOutput);

            // Guardar en archivo con formato legible
            SaveEmailToFile(email, subject, htmlMessage, decodedLink);

            return Task.CompletedTask;
        }

        private void SaveEmailToFile(string email, string subject, string htmlMessage, string resetLink)
        {
            try
            {
                var emailDir = Path.Combine(Directory.GetCurrentDirectory(), "TempEmails");
                Directory.CreateDirectory(emailDir);

                var fileName = $"email_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}.txt";
                var filePath = Path.Combine(emailDir, fileName);

                var emailContent = new StringBuilder()
                    .AppendLine($"Fecha: {DateTime.Now}")
                    .AppendLine($"Para: {email}")
                    .AppendLine($"Asunto: {subject}")
                    .AppendLine()
                    .AppendLine("Enlace de restablecimiento:")
                    .AppendLine(resetLink)
                    .AppendLine()
                    .AppendLine("Contenido HTML:")
                    .AppendLine(htmlMessage)
                    .ToString();

                File.WriteAllText(filePath, emailContent);
                _logger.LogInformation("Correo guardado en: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el correo en archivo");
            }
        }
    }
}