using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JoinIt.Web.Pages
{
    public class ErroModel : PageModel
    {
        public int Codigo { get; private set; }

        public string Titulo { get; private set; }
            = "Ocorreu um erro";

        public string Mensagem { get; private set; }
            = "Não foi possível concluir o pedido.";

        public void OnGet(int codigo)
        {
            Codigo = codigo;

            Response.StatusCode = codigo;

            switch (codigo)
            {
                case 401:
                    Titulo = "Não autorizado";
                    Mensagem =
                        "Precisas de iniciar sessão para aceder a esta página.";
                    break;

                case 403:
                    Titulo = "Acesso negado";
                    Mensagem =
                        "Não tens permissão para aceder a esta página.";
                    break;

                case 404:
                    Titulo = "Página não encontrada";
                    Mensagem =
                        "A página que procuras não existe ou já não está disponível.";
                    break;

                default:
                    Titulo = "Ocorreu um erro";
                    Mensagem =
                        "Não foi possível concluir o pedido.";
                    break;
            }
        }
    }
}