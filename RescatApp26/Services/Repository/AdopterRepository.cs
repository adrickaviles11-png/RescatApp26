using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Entities;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

public class AdopterRepository : IAdopterRepository
{
    private readonly RescatAppDbContext _context;

    private static readonly Expression<Func<Adoptante, AdopterDto>> ToDto = a => new AdopterDto
    {
        Id = a.Id,
        FirstNames = a.Nombres,
        LastNames = a.Apellidos,
        DocumentId = a.DocumentoIdentidad,
        Phone = a.Telefono,
        Email = a.Correo,
        Address = a.Direccion,
        Country = a.PaisOrigen,
        RegisteredAt = a.FechaRegistro
    };

    public AdopterRepository(RescatAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IEnumerable<AdopterDto> GetAllAdopters()
    {
        try
        {
            return _context.Adoptantes.AsNoTracking()
                .OrderBy(a => a.Apellidos).ThenBy(a => a.Nombres).Select(ToDto).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener los adoptantes.", ex);
        }
    }

    public AdopterDto? GetAdopterById(int id)
    {
        try
        {
            return _context.Adoptantes.AsNoTracking().Where(a => a.Id == id).Select(ToDto).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener el adoptante.", ex);
        }
    }

    public int CreateAdopter(AdopterDto adopter)
    {
        if (adopter is null) throw new ArgumentNullException(nameof(adopter));

        try
        {
            if (_context.Adoptantes.AsNoTracking().Any(a => a.DocumentoIdentidad == adopter.DocumentId))
                throw new InvalidOperationException("Ya existe un adoptante con ese documento de identidad.");

            var entity = new Adoptante
            {
                Nombres = adopter.FirstNames,
                Apellidos = adopter.LastNames,
                DocumentoIdentidad = adopter.DocumentId,
                Telefono = adopter.Phone,
                Correo = adopter.Email,
                Direccion = adopter.Address,
                PaisOrigen = adopter.Country,
                FechaRegistro = DateTime.Now
            };
            _context.Adoptantes.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al registrar el adoptante.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    public void UpdateAdopter(AdopterDto adopter)
    {
        if (adopter is null) throw new ArgumentNullException(nameof(adopter));

        try
        {
            var entity = _context.Adoptantes.FirstOrDefault(a => a.Id == adopter.Id)
                ?? throw new InvalidOperationException("El adoptante no existe.");

            if (_context.Adoptantes.AsNoTracking().Any(a => a.DocumentoIdentidad == adopter.DocumentId && a.Id != adopter.Id))
                throw new InvalidOperationException("Ya existe otro adoptante con ese documento de identidad.");

            entity.Nombres = adopter.FirstNames;
            entity.Apellidos = adopter.LastNames;
            entity.DocumentoIdentidad = adopter.DocumentId;
            entity.Telefono = adopter.Phone;
            entity.Correo = adopter.Email;
            entity.Direccion = adopter.Address;
            entity.PaisOrigen = adopter.Country;
            _context.SaveChanges();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al actualizar el adoptante.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    public void DeleteAdopter(int id)
    {
        try
        {
            var entity = _context.Adoptantes.FirstOrDefault(a => a.Id == id)
                ?? throw new InvalidOperationException("El adoptante no existe.");

            if (_context.Citas.AsNoTracking().Any(c => c.AdoptanteId == id))
                throw new InvalidOperationException("No se puede eliminar: el adoptante tiene citas registradas.");

            if (_context.Adopciones.AsNoTracking().Any(a => a.AdoptanteId == id))
                throw new InvalidOperationException("No se puede eliminar: el adoptante tiene adopciones registradas.");

            _context.Adoptantes.Remove(entity);
            _context.SaveChanges();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al eliminar el adoptante.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }
}