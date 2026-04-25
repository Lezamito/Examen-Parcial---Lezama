using System.ComponentModel.DataAnnotations;

namespace Parcial.Models.ViewModels
{
    public class CrearSolicitudViewModel
    {
        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Display(Name = "Monto a Solicitar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
        public decimal MontoSolicitado { get; set; }
    }
}
