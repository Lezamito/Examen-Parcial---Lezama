using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parcial.Models;

namespace Parcial.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<SolicitudCredito> SolicitudesCredito { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Restricción: IngresosMensuales > 0
            builder.Entity<Cliente>()
                .ToTable(t => t.HasCheckConstraint("CK_Cliente_IngresosMensuales", "IngresosMensuales > 0"));

            // Restricción: MontoSolicitado > 0
            builder.Entity<SolicitudCredito>()
                .ToTable(t => t.HasCheckConstraint("CK_Solicitud_MontoSolicitado", "MontoSolicitado > 0"));

            // Índice único: un cliente solo puede tener UNA solicitud Pendiente
            // (Pendiente = 0 en el enum)
            builder.Entity<SolicitudCredito>()
                .HasIndex(s => new { s.ClienteId, s.Estado })
                .HasFilter("Estado = 0")  // 0 = Pendiente
                .IsUnique()
                .HasDatabaseName("UX_SolicitudCredito_ClientePendiente");

            // Relación Cliente -> SolicitudesCredito
            builder.Entity<SolicitudCredito>()
                .HasOne(s => s.Cliente)
                .WithMany(c => c.Solicitudes)
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación ApplicationUser -> Cliente
            builder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
