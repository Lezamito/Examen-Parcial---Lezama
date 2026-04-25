using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parcial.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser mayores a 0.")]
        public decimal IngresosMensuales { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación
        public ApplicationUser? Usuario { get; set; }
        public ICollection<SolicitudCredito> Solicitudes { get; set; } = new List<SolicitudCredito>();
    }
}
