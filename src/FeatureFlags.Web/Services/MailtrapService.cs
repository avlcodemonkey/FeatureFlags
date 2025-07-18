using System.Net;
using System.Net.Mail;

namespace FeatureFlags.Services;

public sealed class MailtrapService(IConfiguration configuration, ILogger<MailtrapService> logger) : IEmailService {
    private readonly IConfiguration _Configuration = configuration;
    private readonly ILogger<MailtrapService> _Logger = logger;

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string html, CancellationToken cancellationToken = default) {
        var settings = _Configuration.GetSection("Mail");

        try {
            var client = new SmtpClient(settings["Host"], int.Parse(settings["Port"]!)) {
                EnableSsl = true,
                UseDefaultCredentials = bool.Parse(settings["DefaultCredentials"]!),
                Credentials = new NetworkCredential(settings["UserName"], settings["Password"])
            };

            var message = new MailMessage {
                From = new MailAddress(settings["FromEmail"]!, settings["FromName"]!)
            };
            message.To.Add(new MailAddress(toEmail, toName));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = html;

            await client.SendMailAsync(message, cancellationToken);

            return true;
        } catch (Exception ex) {
            _Logger.LogError(ex, "Error sending email");
            return false;
        }
    }
}
