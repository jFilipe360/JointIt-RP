using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Enums
{
    public enum EstadoEvento
    {
        [Display(Name = "Para breve")]
        ParaBreve,

        [Display(Name = "A decorrer")]
        ADecorrer,

        [Display(Name = "Terminado")]
        Terminado,

        [Display(Name = "Cancelado")]
        Cancelado
    }
}
