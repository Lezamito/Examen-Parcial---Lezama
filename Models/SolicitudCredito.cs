using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parcial.Models
{
    public enum EstadoSolicitud
    {
        Pendiente,
        Aprobado,
        Rechazado
    }

    public class SolicitudCredito
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
        public decimal MontoSolicitado { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        // Navegación
        public Cliente? Cliente { get; set; }
    }
}
