using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Models.Local;
using RescatApp26.Services.Interfaz_service;
using System.Text.Json;

namespace RescatApp26.Services.Repository;

/// <summary>
/// Envuelve al repositorio normal. Con conexión usa SQL Server; sin conexión lee la copia local
/// y las mascotas nuevas se guardan en la cola FIFO de SQLite.
/// </summary>
public class OfflinePetRepository : IPetRepository
{
    public const string EntityName = "Mascota";
    private const string CacheKey = "mascotas";
    private const string OfflineMessage =
        "Sin conexión: editar y eliminar requieren conexión con el servidor. Mientras tanto solo puede registrar mascotas nuevas.";

    private readonly IPetRepository _inner;
    private readonly IConnectivityService _connectivity;
    private readonly ISyncQueue _queue;

    public OfflinePetRepository(IPetRepository inner, IConnectivityService connectivity, ISyncQueue queue)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public IEnumerable<PetDto> GetAllPets()
    {
        if (_connectivity.IsOnline())
        {
            var pets = _inner.GetAllPets().ToList();
            try { SaveCache(pets); } catch { /* la copia local no debe impedir ver los datos */ }
            return pets.Concat(GetPendingPets()).ToList();
        }

        return LoadCache().Concat(GetPendingPets()).ToList();
    }

    public IEnumerable<PetDto> GetAvailablePets()
    {
        if (_connectivity.IsOnline()) return _inner.GetAvailablePets();
        return LoadCache().Where(p => p.Status == (int)PetStatus.Disponible).ToList();
    }

    public PetDto? GetPetById(int id)
    {
        if (id < 0) return GetPendingPets().FirstOrDefault(p => p.Id == id);
        if (_connectivity.IsOnline()) return _inner.GetPetById(id);
        return LoadCache().FirstOrDefault(p => p.Id == id);
    }

    public int CreatePet(PetDto pet)
    {
        if (pet is null) throw new ArgumentNullException(nameof(pet));
        if (_connectivity.IsOnline()) return _inner.CreatePet(pet);

        ValidateForQueue(pet);
        try
        {
            long queueId = _queue.Enqueue(EntityName, "Crear", JsonSerializer.Serialize(pet));
            return -(int)queueId;   // id negativo = pendiente de enviar
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible guardar la mascota en la cola local.", ex);
        }
    }

    public void UpdatePet(PetDto pet)
    {
        if (pet is null) throw new ArgumentNullException(nameof(pet));
        if (pet.Id < 0)
            throw new InvalidOperationException("Esa mascota todavía está pendiente de enviarse; podrá editarla cuando se sincronice.");
        if (!_connectivity.IsOnline()) throw new InvalidOperationException(OfflineMessage);
        _inner.UpdatePet(pet);
    }

    public void DeletePet(int id)
    {
        if (id < 0)
            throw new InvalidOperationException("Esa mascota todavía está pendiente de enviarse; podrá eliminarla cuando se sincronice.");
        if (!_connectivity.IsOnline()) throw new InvalidOperationException(OfflineMessage);
        _inner.DeletePet(id);
    }

    private List<PetDto> GetPendingPets()
    {
        var result = new List<PetDto>();
        foreach (var item in _queue.GetAll())
        {
            if (item.Estado != QueueStates.Pendiente || item.Entidad != EntityName || item.Operacion != "Crear") continue;

            var pet = JsonSerializer.Deserialize<PetDto>(item.Payload);
            if (pet is null) continue;

            pet.Id = -(int)item.Id;
            pet.Name += " (pendiente)";
            result.Add(pet);
        }
        return result;
    }

    private static void ValidateForQueue(PetDto pet)
    {
        if (!Enum.IsDefined((PetSex)pet.Sex)) throw new InvalidOperationException("El sexo no es válido.");
        if (!Enum.IsDefined((PetSize)pet.Size)) throw new InvalidOperationException("El tamaño no es válido.");
        if (!Enum.IsDefined((PetStatus)pet.Status)) throw new InvalidOperationException("El estado no es válido.");
        if (pet.Status == (int)PetStatus.Adoptada)
            throw new InvalidOperationException("El estado Adoptada solo se asigna al registrar una adopción.");
        if (pet.AgeMonths is < 0 or > 360)
            throw new InvalidOperationException("La edad debe estar entre 0 y 360 meses.");
    }

    private static void SaveCache(List<PetDto> pets)
    {
        using var db = new LocalDbContext();
        var entry = db.Cache.FirstOrDefault(c => c.Clave == CacheKey);
        if (entry is null)
        {
            entry = new CacheItem { Clave = CacheKey };
            db.Cache.Add(entry);
        }
        entry.Json = JsonSerializer.Serialize(pets);
        entry.Actualizado = DateTime.Now;
        db.SaveChanges();
    }

    private static List<PetDto> LoadCache()
    {
        using var db = new LocalDbContext();
        var entry = db.Cache.AsNoTracking().FirstOrDefault(c => c.Clave == CacheKey);
        if (entry is null) return new List<PetDto>();
        return JsonSerializer.Deserialize<List<PetDto>>(entry.Json) ?? new List<PetDto>();
    }
}