using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JoinIt.Web.Services
{
    // Serviço de envio de emails do ASP.NET Core Identity através de SMTP
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage)
        {
            var mensagem = new MimeMessage();

            mensagem.From.Add(
                new MailboxAddress(
                    _settings.FromName,
                    _settings.FromEmail));

            mensagem.To.Add(
                MailboxAddress.Parse(email));

            mensagem.Subject = subject;

            mensagem.Body = new BodyBuilder
            {
                HtmlBody = htmlMessage
            }.ToMessageBody();

            // Liga ao servidor SMTP usando as definições configuradas da aplicação
            using var cliente = new SmtpClient();

            await cliente.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls);

            // Autentica com as credenciais SMTP armazenadas na configuração segura
            await cliente.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            await cliente.SendAsync(mensagem);

            await cliente.DisconnectAsync(true);
        }
    }
}