namespace JoinIt.Web.ViewModels
{
    // Dados necessários para apresentar o avatar de um utilizador
    public class AvatarUtilizadorViewModel
    {
        public string Nome { get; set; } = string.Empty;

        public string? FotoPerfil { get; set; }

        public int Tamanho { get; set; } = 40;
    }
}