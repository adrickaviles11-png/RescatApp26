using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Entities;
using RescatApp26.Models.Enums;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

public class CitaRepository : ICitaRepository
{
    private readonly RescatAppDbContext _context;

    private static readonly Expression<Func<Cita, CitaDto>> ToDto = c => new CitaDto
    {
        Id = c.Id,
        Type = c.Tipo,
        PetId = c.MascotaId,
        VeterinaryId = c.VeterinariaId,
        AdopterId = c.AdoptanteId,
        ScheduledAt = c.FechaHora,
        Reason = c.Motivo,
        Status = c.Estado,
        Notes = c.Observaciones,
        PetName = c.Mascota.Nombre,
        DestinationName = c.Tipo == (int)CitaTipo.Veterinaria
            ? c.Veterinaria!.Nombre
            : c.Adoptante!.Nombres + " " + c.Adoptante!.Apellidos
    };

    public CitaRepository(RescatAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IEnumerable<CitaDto> GetAllCitas()
    {
        try
        {
            return _context.Citas.AsNoTracking().OrderByDescending(c => c.FechaHora).Select(ToDto).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las citas.", ex);
        }
    }

    public CitaDto? GetCitaById(int id)
    {
        try
        {
            return _context.Citas.AsNoTracking().Where(c => c.Id == id).Select(ToDto).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener la cita.", ex);
        }
    }

    public IEnumerable<LookupItemDto> GetPetsForCitas()
    {
        try
        {
            int adopted = (int)PetStatus.Adoptada;
            return _context.Mascotas.AsNoTracking()
                .Where(m => m.Estado != adopted)
                .OrderBy(m => m.Nombre)
                .Select(m => new LookupItemDto { Id = m.Id, Text = m.Nombre + " (" + m.Especie + ")" })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las mascotas.", ex);
        }
    }

    public IEnumerable<LookupItemDto> GetVeterinarias()
    {
        try
        {
            return _context.Veterinarias.AsNoTracking()
                .Where(v => v.Activa)
                .OrderBy(v => v.Nombre)
                .Select(v => new LookupItemDto { Id = v.Id, Text = v.Nombre })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las veterinarias.", ex);
        }
    }

    public int CreateCita(CitaDto cita)
    {
        if (cita is null) throw new ArgumentNullException(nameof(cita));
        DateTime when = TrimToMinute(cita.ScheduledAt);
        Validate(cita, when);

        try
        {
            EnsurePetNotAdopted(cita.PetId);
            EnsureNoConflict(cita.PetId, when, cita.Status, 0);

            var entity = new Cita
            {
                Tipo = cita.Type,
                MascotaId = cita.PetId,
                VeterinariaId = cita.Type == (int)CitaTipo.Veterinaria ? cita.VeterinaryId : null,
                AdoptanteId = cita.Type == (int)CitaTipo.VisitaAdopcion ? cita.AdopterId : null,
                FechaHora = when,
                Motivo = cita.Reason,
                Estado = cita.Status,
                Observaciones = cita.Notes,
                FechaRegistro = DateTime.Now
            };
            _context.Citas.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al registrar la cita.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    public void UpdateCita(CitaDto cita)
    {
        if (cita is null) throw new ArgumentNullException(nameof(cita));
        DateTime when = TrimToMinute(cita.ScheduledAt);
        Validate(cita, when);

        try
        {
            var entity = _context.Citas.FirstOrDefault(c => c.Id == cita.Id)
                ?? throw new InvalidOperationException("La cita no existe.");

            if (entity.Estado == (int)CitaEstado.Completada || entity.Estado == (int)CitaEstado.Cancelada)
                throw new InvalidOperationException("Una cita completada o cancelada ya no se puede modificar.");

            if (entity.MascotaId != cita.PetId) EnsurePetNotAdopted(cita.PetId);
            EnsureNoConflict(cita.PetId, when, cita.Status, cita.Id);

            entity.Tipo = cita.Type;
            entity.MascotaId = cita.PetId;
            entity.VeterinariaId = cita.Type == (int)CitaTipo.Veterinaria ? cita.VeterinaryId : null;
            entity.AdoptanteId = cita.Type == (int)CitaTipo.VisitaAdopcion ? cita.AdopterId : null;
            entity.FechaHora = when;
            entity.Motivo = cita.Reason;
            entity.Estado = cita.Status;
            entity.Observaciones = cita.Notes;
            _context.SaveChanges();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al actualizar la cita.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    public void CancelCita(int id)
    {
        try
        {
            var entity = _context.Citas.FirstOrDefault(c => c.Id == id)
                ?? throw new InvalidOperationException("La cita no existe.");

            if (entity.Estado == (int)CitaEstado.Cancelada)
                throw new InvalidOperationException("La cita ya está cancelada.");
            if (entity.Estado == (int)CitaEstado.Completada)
                throw new InvalidOperationException("Una cita completada no se puede cancelar.");

            entity.Estado = (int)CitaEstado.Cancelada;
            _context.SaveChanges();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al cancelar la cita.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    private static DateTime TrimToMinute(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);

    private static void Validate(CitaDto cita, DateTime when)
    {
        if (!Enum.IsDefined((CitaTipo)cita.Type)) throw new InvalidOperationException("El tipo de cita no es válido.");
        if (!Enum.IsDefined((CitaEstado)cita.Status)) throw new InvalidOperationException("El estado de la cita no es válido.");
        if (cita.Type == (int)CitaTipo.Veterinaria && cita.VeterinaryId is null)
            throw new InvalidOperationException("Seleccione la veterinaria.");
        if (cita.Type == (int)CitaTipo.VisitaAdopcion && cita.AdopterId is null)
            throw new InvalidOperationException("Seleccione el adoptante.");

        bool active = cita.Status == (int)CitaEstado.Programada || cita.Status == (int)CitaEstado.Confirmada;
        if (active && when < DateTime.Now)
            throw new InvalidOperationException("La fecha de una cita programada o confirmada no puede estar en el pasado.");
    }

    private void EnsurePetNotAdopted(int petId)
    {
        int? state = _context.Mascotas.AsNoTracking()
            .Where(m => m.Id == petId).Select(m => (int?)m.Estado).FirstOrDefault();

        if (state is null) throw new InvalidOperationException("La mascota seleccionada no existe.");
        if (state == (int)PetStatus.Adoptada)
            throw new InvalidOperationException("La mascota ya fue adoptada; no se puede programar una cita.");
    }

    // Misma regla que el índice único UX_Citas_Mascota_Hora, con un mensaje claro.
    private void EnsureNoConflict(int petId, DateTime when, int status, int excludeId)
    {
        int scheduled = (int)CitaEstado.Programada;
        int confirmed = (int)CitaEstado.Confirmada;
        if (status != scheduled && status != confirmed) return;

        bool exists = _context.Citas.AsNoTracking().Any(c =>
            c.MascotaId == petId && c.FechaHora == when &&
            (c.Estado == scheduled || c.Estado == confirmed) && c.Id != excludeId);

        if (exists) throw new InvalidOperationException("La mascota ya tiene una cita vigente a esa hora.");
    }
}