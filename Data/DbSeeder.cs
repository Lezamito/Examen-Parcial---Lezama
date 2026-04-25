using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parcial.Models;

namespace Parcial.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplicar migraciones pendientes automáticamente
            await context.Database.MigrateAsync();

            // ─── 1. Roles ────────────────────────────────────────────────────────
            string[] roles = { "Analista", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ─── 2. Usuario Analista ─────────────────────────────────────────────
            const string analistaEmail = "analista@creditos.com";
            if (await userManager.FindByEmailAsync(analistaEmail) == null)
            {
                var analista = new ApplicationUser
                {
                    UserName = analistaEmail,
                    Email = analistaEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(analista, "Analista@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(analista, "Analista");
            }

            // ─── 3. Usuarios Cliente ─────────────────────────────────────────────
            const string cliente1Email = "juan.perez@cliente.com";
            const string cliente2Email = "maria.garcia@cliente.com";

            ApplicationUser? userCliente1 = await userManager.FindByEmailAsync(cliente1Email);
            if (userCliente1 == null)
            {
                userCliente1 = new ApplicationUser
                {
                    UserName = cliente1Email,
                    Email = cliente1Email,
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(userCliente1, "Cliente@123");
                if (r.Succeeded)
                    await userManager.AddToRoleAsync(userCliente1, "Cliente");
            }

            ApplicationUser? userCliente2 = await userManager.FindByEmailAsync(cliente2Email);
            if (userCliente2 == null)
            {
                userCliente2 = new ApplicationUser
                {
                    UserName = cliente2Email,
                    Email = cliente2Email,
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(userCliente2, "Cliente@123");
                if (r.Succeeded)
                    await userManager.AddToRoleAsync(userCliente2, "Cliente");
            }

            // Re-fetch para tener IDs actualizados
            userCliente1 = await userManager.FindByEmailAsync(cliente1Email);
            userCliente2 = await userManager.FindByEmailAsync(cliente2Email);

            // ─── 4. Clientes ─────────────────────────────────────────────────────
            if (!await context.Clientes.AnyAsync())
            {
                var clientes = new List<Cliente>
                {
                    new Cliente
                    {
                        UsuarioId       = userCliente1!.Id,
                        IngresosMensuales = 3000m,
                        Activo          = true
                    },
                    new Cliente
                    {
                        UsuarioId       = userCliente2!.Id,
                        IngresosMensuales = 5000m,
                        Activo          = true
                    }
                };
                context.Clientes.AddRange(clientes);
                await context.SaveChangesAsync();
            }

            // ─── 5. Solicitudes de Crédito ───────────────────────────────────────
            if (!await context.SolicitudesCredito.AnyAsync())
            {
                var cliente1 = await context.Clientes
                    .FirstAsync(c => c.UsuarioId == userCliente1!.Id);
                var cliente2 = await context.Clientes
                    .FirstAsync(c => c.UsuarioId == userCliente2!.Id);

                var solicitudes = new List<SolicitudCredito>
                {
                    // Solicitud PENDIENTE — Juan Pérez (monto <= 5x ingresos: 3000*5=15000)
                    new SolicitudCredito
                    {
                        ClienteId       = cliente1.Id,
                        MontoSolicitado = 8000m,
                        FechaSolicitud  = DateTime.UtcNow.AddDays(-3),
                        Estado          = EstadoSolicitud.Pendiente,
                        MotivoRechazo   = null
                    },
                    // Solicitud APROBADA — María García (monto <= 5x ingresos: 5000*5=25000)
                    new SolicitudCredito
                    {
                        ClienteId       = cliente2.Id,
                        MontoSolicitado = 12000m,
                        FechaSolicitud  = DateTime.UtcNow.AddDays(-10),
                        Estado          = EstadoSolicitud.Aprobado,
                        MotivoRechazo   = null
                    }
                };
                context.SolicitudesCredito.AddRange(solicitudes);
                await context.SaveChangesAsync();
            }
        }
    }
}
