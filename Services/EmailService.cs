using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LandiGlobalTemplate.Services
{
    /// <summary>
    /// Service d'envoi d'emails via SMTP
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Envoie un email simple
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];

                if (string.IsNullOrWhiteSpace(smtpServer) ||
                    string.IsNullOrWhiteSpace(smtpUsername) ||
                    string.IsNullOrWhiteSpace(smtpPassword) ||
                    smtpUsername == "your-email@gmail.com" ||
                    smtpPassword == "your-app-password")
                {
                    _logger.LogWarning("Configuration SMTP manquante");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Landi Global", smtpUsername));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                message.Body = new TextPart(isHtml ? "html" : "plain")
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email envoyé avec succès à {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'envoi d'email à {to}");
                return false;
            }
        }

        /// <summary>
        /// Envoie le code de confirmation après création de compte
        /// </summary>
        public async Task<bool> SendVerificationCodeAsync(string to, string firstName, string code)
        {
            var subject = "Votre code de vérification - Landi Global";
            var displayName = string.IsNullOrWhiteSpace(firstName) ? "Bonjour" : $"Bonjour {firstName}";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 24px; background: #f9f9f9; }}
        .code {{ background: white; border: 1px solid #e2e2e2; border-radius: 8px; color: #667eea; font-size: 32px; font-weight: bold; letter-spacing: 6px; margin: 20px 0; padding: 18px; text-align: center; }}
        .footer {{ text-align: center; padding: 10px; color: #999; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Vérification du compte</h1>
        </div>
        <div class=""content"">
            <p>{displayName},</p>
            <p>Utilisez ce code pour confirmer votre compte Landi Global :</p>
            <div class=""code"">{code}</div>
            <p>Ce code expire dans 15 minutes.</p>
            <p>Si vous n'avez pas créé ce compte, vous pouvez ignorer cet email.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Landi Global. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(to, subject, body, isHtml: true);
        }

        /// <summary>
        /// Envoie un email avec une pièce jointe
        /// </summary>
        public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, 
            byte[] attachmentData, string attachmentFileName, string attachmentMediaType = "application/octet-stream")
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Landi Global", smtpUsername));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var multipart = new Multipart("mixed");
                
                // Corps de l'email
                multipart.Add(new TextPart("html") { Text = body });

                // Pièce jointe
                var attachment = new MimePart(attachmentMediaType)
                {
                    Content = new MimeContent(new MemoryStream(attachmentData)),
                    ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachmentFileName
                };
                multipart.Add(attachment);

                message.Body = multipart;

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email avec pièce jointe envoyé à {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'envoi d'email avec pièce jointe à {to}");
                return false;
            }
        }

        /// <summary>
        /// Envoie une notification de facture créée
        /// </summary>
        public async Task<bool> SendInvoiceNotificationAsync(string customerEmail, string invoiceNumber, decimal amount, string currency)
        {
            var subject = $"Facture {invoiceNumber} - Landi Global";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .footer {{ text-align: center; padding: 10px; color: #999; font-size: 12px; }}
        .invoice-details {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .amount {{ font-size: 24px; color: #667eea; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📋 Nouvelle Facture</h1>
        </div>
        <div class=""content"">
            <p>Bonjour,</p>
            <p>Une nouvelle facture a été créée pour votre compte.</p>
            <div class=""invoice-details"">
                <p><strong>N° Facture:</strong> {invoiceNumber}</p>
                <p><strong>Montant:</strong> <span class=""amount"">{amount:F2} {currency}</span></p>
                <p><strong>Date:</strong> {DateTime.Now:dd/MM/yyyy}</p>
            </div>
            <p>Pour consulter les détails de votre facture, veuillez vous connecter à votre compte.</p>
            <p>Cordialement,<br/>L'équipe Landi Global</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Landi Global. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(customerEmail, subject, body, isHtml: true);
        }

        /// <summary>
        /// Envoie une notification de validation
        /// </summary>
        public async Task<bool> SendValidationNotificationAsync(string userEmail, string invoiceNumber, bool isValid, List<string>? errors = null)
        {
            var subject = $"Validation - Facture {invoiceNumber}";
            var statusText = isValid ? "✓ Validée avec succès" : "✗ Erreurs de validation";
            var statusColor = isValid ? "#28a745" : "#dc3545";

            var errorList = "";
            if (errors?.Count > 0)
            {
                errorList = "<ul>";
                foreach (var error in errors)
                {
                    errorList += $"<li>{error}</li>";
                }
                errorList += "</ul>";
            }

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .status {{ color: {statusColor}; font-size: 18px; font-weight: bold; }}
        .error-list {{ background: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📊 Résultat de Validation</h1>
        </div>
        <div class=""content"">
            <p>Bonjour,</p>
            <p><strong>Facture:</strong> {invoiceNumber}</p>
            <p><strong>Statut:</strong> <span class=""status"">{statusText}</span></p>
            {(errors?.Count > 0 ? $"<div class=\"error-list\"><strong>Erreurs trouvées:</strong>{errorList}</div>" : "<p>Aucune erreur détectée.</p>")}
            <p>Cordialement,<br/>L'équipe Landi Global</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(userEmail, subject, body, isHtml: true);
        }

        /// <summary>
        /// Envoie un rapport d'export
        /// </summary>
        public async Task<bool> SendExportReportAsync(string userEmail, string exportType, int count, byte[]? attachmentData = null)
        {
            var subject = $"Export {exportType} - {count} enregistrements";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background: #667eea; color: white; padding: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📤 Export Complété</h1>
        </div>
        <div style=""padding: 20px;"">
            <p>Votre export {exportType} a été généré avec succès.</p>
            <p><strong>Nombre d'enregistrements:</strong> {count}</p>
            <p>Le fichier est attaché à cet email.</p>
            <p>Cordialement,<br/>L'équipe Landi Global</p>
        </div>
    </div>
</body>
</html>";

            if (attachmentData != null)
            {
                var fileName = $"Export_{exportType}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
                return await SendEmailWithAttachmentAsync(userEmail, subject, body, attachmentData, fileName, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }

            return await SendEmailAsync(userEmail, subject, body, isHtml: true);
        }
    }
}
