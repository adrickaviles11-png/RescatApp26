using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly RescatAppDbContext _context;

    public DashboardRepository(RescatAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public DashboardDto GetSummary(DateTime today)
    {
        try
        {
            DateTime day = today.Date;
            DateTime weekStart = day.AddDays(-(((int)day.DayOfWeek + 6) % 7)); // lunes
            DateTime weekEnd = weekStart.AddDays(7);
            DateTime monthStart = new(day.Year, day.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1);
            DateTime halfEnd = monthStart.AddDays(15); // del 1 al 15 inclusive

            int available = (int)PetStatus.Disponible;
            int adopted = (int)PetStatus.Adoptada;
            var citas = _context.Citas.AsNoTracking();

            var summary = new DashboardDto
            {
                AvailablePets = _context.Mascotas.AsNoTracking().Count(m => m.Estado == available),
                AdoptedPets = _context.Mascotas.AsNoTracking().Count(m => m.Estado == adopted),
                Adopters = _context.Adoptantes.AsNoTracking().Count(),
                AdoptionsThisMonth = _context.Adopciones.AsNoTracking()
                    .Count(a => a.FechaAdopcion >= monthStart && a.FechaAdopcion < monthEnd),
                CitasThisWeek = citas.Count(c => c.FechaHora >= weekStart && c.FechaHora < weekEnd),
                CitasFirstHalf = citas.Count(c => c.FechaHora >= monthStart && c.FechaHora < halfEnd),
                CitasThisMonth = citas.Count(c => c.FechaHora >= monthStart && c.FechaHora < monthEnd)
            };

            var byStatus = citas.GroupBy(c => c.Estado)
                .Select(g => new { Key = g.Key, Count = g.Count() }).ToList();
            summary.CitasByStatus = CitaOptions.Statuses
                .Select(o => new ChartItemDto
                {
                    Label = o.Text,
                    Value = byStatus.FirstOrDefault(x => x.Key == o.Value)?.Count ?? 0
                }).ToList();

            var byType = citas.GroupBy(c => c.Tipo)
                .Select(g => new { Key = g.Key, Count = g.Count() }).ToList();
            summary.CitasByType = CitaOptions.Types
                .Select(o => new ChartItemDto
                {
                    Label = o.Text,
                    Value = byType.FirstOrDefault(x => x.Key == o.Value)?.Count ?? 0
                }).ToList();

            return summary;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible calcular el resumen.", ex);
        }
    }
}