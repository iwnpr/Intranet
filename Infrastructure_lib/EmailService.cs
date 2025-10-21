using Application_lib;
using Domain_lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_lib
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }
        public async Task SendOrganizationRequestAsync(OrganizationRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));


            if (string.IsNullOrWhiteSpace(_settings.Host))
                throw new InvalidOperationException("Host is not configured.");
            

            var recipient = string.IsNullOrWhiteSpace(_settings.OrganizationRequestRecipient)
                //? "help@rs-cb.ru"
                ? "id.piryutko@bki-ci.ru"
                : _settings.OrganizationRequestRecipient!;

            using var client = CreateClient();
            using var message = BuildMessage(request, recipient);

            try
            {
                await client.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send organization request email");
                throw;
            }

        }

        private SmtpClient CreateClient()
        {
            var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_settings.Username))
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            
            return client;
        }

        private MailMessage BuildMessage(OrganizationRequestMessage request, string recipient)
        {
            var fromAddress = !string.IsNullOrWhiteSpace(_settings.From)
                ? new MailAddress(_settings.From!)
                : !string.IsNullOrWhiteSpace(_settings.Username)
                    ? new MailAddress(_settings.Username!)
                    : new MailAddress("no-reply@localhost");

            var message = new MailMessage
            {
                From = fromAddress,
                Subject = $"Заявка на подключение организации: {request.OrganizationName}",
                Body = BuildBody(request),
                IsBodyHtml = false
            };

            message.To.Add(recipient);

            return message;
        }

        private static string BuildBody(OrganizationRequestMessage request)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Получена новая заявка на заведение организации.");
            builder.AppendLine();
            builder.AppendLine($"Наименование организации: {request.OrganizationName}");
            builder.AppendLine($"ИНН: {request.Inn}");
            builder.AppendLine($"ОГРН: {request.Ogrn}");
            builder.AppendLine();
            builder.AppendLine("IP-адреса для white-листов:");
            foreach (var ip in request.WhiteListIps)
            {
                builder.AppendLine($" - {ip}");
            }

            builder.AppendLine();
            builder.AppendLine("Запрошенные сервисы:");
            foreach (var service in request.Services)
            {
                builder.AppendLine($" - {service}");
            }

            return builder.ToString();
        }
    }
}
