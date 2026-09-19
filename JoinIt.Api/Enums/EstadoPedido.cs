using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Enums
{
    public enum EstadoPedido
    {
        [Display(Name = "Pendente")]
        Pendente,

        [Display(Name = "Aceite")]
        Aceite,

        [Display(Name = "Rejeitado")]
        Rejeitado
    }
}
