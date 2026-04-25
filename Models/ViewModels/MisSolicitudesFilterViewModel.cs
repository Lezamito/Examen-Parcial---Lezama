using System.ComponentModel.DataAnnotations;

namespace Parcial.Models.ViewModels
{
    public class MisSolicitudesFilterViewModel : IValidatableObject
    {
        public EstadoSolicitud? Estado { get; set; }

        [Display(Name = "Monto Mínimo")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
        public decimal? MontoMin { get; set; }

        [Display(Name = "Monto Máximo")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
        public decimal? MontoMax { get; set; }

        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        // Propiedad para enviar las solicitudes a la vista
        public IEnumerable<SolicitudCredito>? Solicitudes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaInicio.HasValue && FechaFin.HasValue)
            {
                if (FechaInicio.Value > FechaFin.Value)
                {
                    yield return new ValidationResult(
                        "La fecha de inicio no puede ser mayor a la fecha de fin.",
                        new[] { nameof(FechaInicio), nameof(FechaFin) }
                    );
                }
            }

            if (MontoMin.HasValue && MontoMax.HasValue)
            {
                if (MontoMin.Value > MontoMax.Value)
                {
                    yield return new ValidationResult(
                        "El monto mínimo no puede ser mayor al monto máximo.",
                        new[] { nameof(MontoMin), nameof(MontoMax) }
                    );
                }
            }
        }
    }
}
