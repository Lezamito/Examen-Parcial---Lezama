using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial.Data;
using Parcial.Models;
using Parcial.Models.ViewModels;
using System.Security.Claims;

namespace Parcial.Controllers
{
    [Authorize]
    public class SolicitudesCreditoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SolicitudesCreditoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisSolicitudes(MisSolicitudesFilterViewModel filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            // Si el modelo no es válido (por los filtros), devolvemos la vista vacía o con errores
            if (!ModelState.IsValid)
            {
                filter.Solicitudes = new List<SolicitudCredito>();
                return View(filter);
            }

            // Consulta base: solicitudes del cliente autenticado
            var query = _context.SolicitudesCredito
                .Where(s => s.ClienteId == cliente.Id)
                .AsQueryable();

            // Aplicar filtros
            if (filter.Estado.HasValue)
            {
                query = query.Where(s => s.Estado == filter.Estado.Value);
            }

            if (filter.MontoMin.HasValue)
            {
                query = query.Where(s => s.MontoSolicitado >= filter.MontoMin.Value);
            }

            if (filter.MontoMax.HasValue)
            {
                query = query.Where(s => s.MontoSolicitado <= filter.MontoMax.Value);
            }

            if (filter.FechaInicio.HasValue)
            {
                query = query.Where(s => s.FechaSolicitud >= filter.FechaInicio.Value);
            }

            if (filter.FechaFin.HasValue)
            {
                // Incluir todo el día final
                var endOfDay = filter.FechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.FechaSolicitud <= endOfDay);
            }

            filter.Solicitudes = await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();

            return View(filter);
        }

        [Authorize]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAnalista = User.IsInRole("Analista");

            var solicitud = await _context.SolicitudesCredito
                .Include(s => s.Cliente)
                .ThenInclude(c => c!.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            // Si no es analista, validar que la solicitud pertenezca al usuario actual
            if (!isAnalista && solicitud.Cliente?.UsuarioId != userId)
            {
                return Forbid();
            }

            return View(solicitud);
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Crear()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            
            if (cliente == null || !cliente.Activo)
            {
                TempData["Error"] = "Su perfil de cliente no está activo o no existe.";
                return RedirectToAction(nameof(MisSolicitudes));
            }

            // Verificar si ya tiene una solicitud pendiente
            bool tienePendiente = await _context.SolicitudesCredito
                .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == EstadoSolicitud.Pendiente);

            if (tienePendiente)
            {
                TempData["Error"] = "Ya tiene una solicitud en estado Pendiente. No puede registrar otra.";
                return RedirectToAction(nameof(MisSolicitudes));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Crear(CrearSolicitudViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);

            // Validación: Cliente activo
            if (cliente == null || !cliente.Activo)
            {
                ModelState.AddModelError(string.Empty, "Su perfil de cliente no está activo o no existe.");
                return View(model);
            }

            // Validación: Solo una solicitud pendiente
            bool tienePendiente = await _context.SolicitudesCredito
                .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == EstadoSolicitud.Pendiente);

            if (tienePendiente)
            {
                ModelState.AddModelError(string.Empty, "Ya tiene una solicitud en estado Pendiente. No puede registrar otra.");
                return View(model);
            }

            // Validación: Monto <= 10 * IngresosMensuales
            decimal limiteMonto = cliente.IngresosMensuales * 10;
            if (model.MontoSolicitado > limiteMonto)
            {
                ModelState.AddModelError("MontoSolicitado", $"El monto solicitado no puede superar 10 veces sus ingresos mensuales ({limiteMonto:C}).");
                return View(model);
            }

            // Crear solicitud
            var solicitud = new SolicitudCredito
            {
                ClienteId = cliente.Id,
                MontoSolicitado = model.MontoSolicitado,
                FechaSolicitud = DateTime.UtcNow,
                Estado = EstadoSolicitud.Pendiente
            };

            _context.SolicitudesCredito.Add(solicitud);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Su solicitud de crédito ha sido registrada exitosamente y está pendiente de evaluación.";
            return RedirectToAction(nameof(MisSolicitudes));
        }

        // Action placeholder para Index del Analista (evitar error en el Layout)
        [Authorize(Roles = "Analista")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
