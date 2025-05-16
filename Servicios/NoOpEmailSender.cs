using Microsoft.AspNetCore.Identity.UI.Services;

namespace Alquileres.Services;

public class NoOpEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}