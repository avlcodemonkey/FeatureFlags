namespace FeatureFlags.Services;

/// <summary>
/// Provides email sending operations for the application.
/// </summary>
public interface IEmailService {
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="html">HTML content of email body.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if email was sent successfully; otherwise, false.</returns>
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string html, CancellationToken cancellationToken = default);
}
