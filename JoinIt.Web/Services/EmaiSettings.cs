namespace JoinIt.Web.Services
{
    // Definições usadas pelo serviço de envio de emails por SMTP
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        // As credenciais devem ser fornecidas através da configuração segura da aplicação
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = "JoinIt";
    }
}