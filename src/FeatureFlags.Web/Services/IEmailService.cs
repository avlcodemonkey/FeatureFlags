namespace FeatureFlags.Services;

public interface IEmailService {
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string html, CancellationToken cancellationToken = default);
}
