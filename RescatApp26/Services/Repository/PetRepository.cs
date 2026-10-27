using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Entities;
using RescatApp26.Models.Enums;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

public class PetRepository : IPetRepository
{
    private readonly RescatAppDbContext _context;

    private static readonly Expression<Func<Mascota, PetDto>> ToDto = m => new PetDto
    {
        Id = m.Id,
        Name = m.Nombre,
        Species = m.Especie,
        Breed = m.Raza,
        AgeMonths = m.EdadMeses,
        Sex = m.Sexo,
        Size = m.Tamano,
        Description = m.Descripcion,
        EntryDate = m.FechaIngreso,
        Status = m.Estado
    };

    public PetRepository(RescatAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IEnumerable<PetDto> GetAllPets()
    {
        try
        {
            return _context.Mascotas.AsNoTracking().OrderBy(m => m.Nombre).Select(ToDto).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las mascotas.", ex);
        }
    }

    public IEnumerable<PetDto> GetAvailablePets()
    {
        try
        {
            int available = (int)PetStatus.Disponible;
            return _context.Mascotas.AsNoTracking()
                .Where(m => m.Estado == available)
                .OrderBy(m => m.Nombre).Select(ToDto).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener las mascotas disponibles.", ex);
        }
    }

    public PetDto? GetPetById(int id)
    {
        try
        {
            return _context.Mascotas.AsNoTracking().Where(m => m.Id == id).Select(ToDto).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible obtener la mascota.", ex);
        }
    }

    public int CreatePet(PetDto pet)
    {
        if (pet is null) throw new ArgumentNullException(nameof(pet));
        ValidateCodes(pet);
        if (pet.Status == (int)PetStatus.Adoptada)
            throw new InvalidOperationException("El estado Adoptada solo se asigna al registrar una adopción.");

        try
        {
            var entity = new Mascota
            {
                Nombre = pet.Name,
                Especie = pet.Species,
                Raza = pet.Breed,
                EdadMeses = pet.AgeMonths,
                Sexo = pet.Sex,
                Tamano = pet.Size,
                Descripcion = pet.Description,
                FechaIngreso = DateTime.Now,
                Estado = pet.Status
            };
            _context.Mascotas.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al registrar la mascota.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    public void UpdatePet(PetDto pet)
    {
        if (pet is null) throw new ArgumentNullException(nameof(pet));
        ValidateCodes(pet);

        try
        {
            var entity = _context.Mascotas.FirstOrDefault(m => m.Id == pet.Id)
                ?? throw new InvalidOperationException("La mascota no existe.");

            // Una mascota adoptada conserva su estado; ese estado solo lo asigna una adopción.
            if (entity.Estado != (int)PetStatus.Adoptada)
            {
                if (pet.Status == (int)PetStatus.Adoptada)
                    throw new InvalidOperationException("El estado Adoptada solo se asigna al registrar una adopción.");
                entity.Estado = pet.Status;
            }

            entity.Nombre = pet.Name;
            entity.Especie = pet.Species;
            entity.Raza = pet.Breed;
            entity.EdadMeses = pet.AgeMonths;
            entity.Sexo = pet.Sex;
            entity.Tamano = pet.Size;
            entity.Descripcion = pet.Description;
            _context.SaveChanges();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al actualizar la mascota.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    public void DeletePet(int id)
    {
        try
        {
            var entity = _context.Mascotas.FirstOrDefault(m => m.Id == id)
                ?? throw new InvalidOperationException("La mascota no existe.");

            if (_context.Adopciones.AsNoTracking().Any(a => a.MascotaId == id))
                throw new InvalidOperationException("No se puede eliminar: la mascota tiene una adopción registrada.");

            _context.Mascotas.Remove(entity);
            _context.SaveChanges();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al eliminar la mascota.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    // Tu base no tiene restricciones CHECK, así que la aplicación es quien valida los códigos.
    private static void ValidateCodes(PetDto pet)
    {
        if (!Enum.IsDefined((PetSex)pet.Sex)) throw new InvalidOperationException("El sexo no es válido.");
        if (!Enum.IsDefined((PetSize)pet.Size)) throw new InvalidOperationException("El tamaño no es válido.");
        if (!Enum.IsDefined((PetStatus)pet.Status)) throw new InvalidOperationException("El estado no es válido.");
    }
}