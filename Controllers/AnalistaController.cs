using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Parcial.Data;
using Parcial.Models;

namespace Parcial.Controllers
{
    [Authorize(Roles = "Analista")]
    public class AnalistaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public AnalistaController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Analista
        public async Task<IActionResult> Index()
        {
            // Solo listar solicitudes en estado Pendiente
            var solicitudesPendientes = await _context.SolicitudesCredito
                .Include(s => s.Cliente)
                .ThenInclude(c => c!.Usuario)
                .Where(s => s.Estado == EstadoSolicitud.Pendiente)
                .OrderBy(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudesPendientes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var solicitud = await _context.SolicitudesCredito
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            // Regla de Negocio: No procesar si ya no está Pendiente
            if (solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                TempData["Error"] = $"La solicitud #{id} ya fue procesada anteriormente.";
                return RedirectToAction(nameof(Index));
            }

            // Regla de Negocio: No aprobar si el monto > 5 * ingresos
            if (solicitud.Cliente != null)
            {
                decimal maximoPermitido = solicitud.Cliente.IngresosMensuales * 5;
                if (solicitud.MontoSolicitado > maximoPermitido)
                {
                    TempData["Error"] = $"No se puede aprobar la solicitud #{id}. El monto solicitado ({solicitud.MontoSolicitado:C}) supera 5 veces los ingresos mensuales del cliente ({maximoPermitido:C}). Debe ser rechazada.";
                    return RedirectToAction(nameof(Index));
                }
            }

            solicitud.Estado = EstadoSolicitud.Aprobado;
            await _context.SaveChangesAsync();

            // Invalidar el caché del cliente para que vea el cambio
            if (solicitud.Cliente != null && !string.IsNullOrEmpty(solicitud.Cliente.UsuarioId))
            {
                await _cache.RemoveAsync($"solicitudes_{solicitud.Cliente.UsuarioId}");
            }

            TempData["Success"] = $"La solicitud #{id} ha sido Aprobada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            if (string.IsNullOrWhiteSpace(motivoRechazo))
            {
                TempData["Error"] = "El motivo de rechazo es obligatorio.";
                return RedirectToAction(nameof(Index));
            }

            var solicitud = await _context.SolicitudesCredito
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            // Regla de Negocio: No procesar si ya no está Pendiente
            if (solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                TempData["Error"] = $"La solicitud #{id} ya fue procesada anteriormente.";
                return RedirectToAction(nameof(Index));
            }

            solicitud.Estado = EstadoSolicitud.Rechazado;
            solicitud.MotivoRechazo = motivoRechazo;
            await _context.SaveChangesAsync();

            // Invalidar el caché del cliente para que vea el cambio
            if (solicitud.Cliente != null && !string.IsNullOrEmpty(solicitud.Cliente.UsuarioId))
            {
                await _cache.RemoveAsync($"solicitudes_{solicitud.Cliente.UsuarioId}");
            }

            TempData["Success"] = $"La solicitud #{id} ha sido Rechazada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
