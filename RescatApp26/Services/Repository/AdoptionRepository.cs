using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Entities;
using RescatApp26.Models.Enums;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

public class AdoptionRepository : IAdoptionRepository
{
    private readonly RescatAppDbContext _context;

    public AdoptionRepository(RescatAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IEnumerable<LookupItemDto> GetAdoptablePets()
    {
        try
        {
            int available = (int)PetStatus.Disponible;
            int inProcess = (int)PetStatus.EnProceso;
            return _context.Mascotas.AsNoTracking()
                .Where(m => m.Estado == available || m.Estado == inProcess)
                .OrderBy(m => m.Nombre)
                .Select(m => new LookupItemDto { Id = m.Id, Text = m.Nombre + " (" + m.Especie + ")" })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las mascotas para adopción.", ex);
        }
    }

    public IEnumerable<AdoptionListDto> GetAllAdoptions()
    {
        try
        {
            return _context.Adopciones.AsNoTracking()
                .OrderByDescending(a => a.FechaAdopcion)
                .Select(a => new AdoptionListDto
                {
                    Id = a.Id,
                    Code = a.CodigoAdopcion,
                    PetName = a.Mascota.Nombre,
                    AdopterName = a.Adoptante.Nombres + " " + a.Adoptante.Apellidos,
                    Date = a.FechaAdopcion,
                    Cost = a.CostoAdopcion,
                    Notes = a.Observaciones
                })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las adopciones.", ex);
        }
    }

    public int CreateAdoption(AdoptionCreateDto adoption)
    {
        if (adoption is null) throw new ArgumentNullException(nameof(adoption));
        if (adoption.Cost < 0)
            throw new InvalidOperationException("El costo de adopción no puede ser negativo.");

        try
        {
            var pet = _context.Mascotas.FirstOrDefault(m => m.Id == adoption.PetId)
                ?? throw new InvalidOperationException("La mascota seleccionada no existe.");

            if (pet.Estado == (int)PetStatus.Adoptada)
                throw new InvalidOperationException("La mascota ya fue adoptada.");

            if (!_context.Adoptantes.AsNoTracking().Any(a => a.Id == adoption.AdopterId))
                throw new InvalidOperationException("El adoptante seleccionado no existe.");

            var entity = new Adopcion
            {
                MascotaId = pet.Id,
                AdoptanteId = adoption.AdopterId,
                FechaAdopcion = DateTime.Now,
                Observaciones = adoption.Notes,
                CostoAdopcion = adoption.Cost
            };

            // Un solo SaveChanges = una sola transacción: se guardan las dos cosas o ninguna.
            pet.Estado = (int)PetStatus.Adoptada;
            _context.Adopciones.Add(entity);
            _context.SaveChanges();

            return entity.CodigoAdopcion; // lo asignó la secuencia SeqCodigoAdopcion
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al registrar la adopción.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }
}