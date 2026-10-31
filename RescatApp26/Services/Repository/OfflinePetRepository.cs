using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Models.Local;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

/// <summary>
/// Envuelve al repositorio normal. Con conexión usa SQL Server; si la conexión se pierde (aunque sea
/// en medio de una operación) pasa solo a la copia local, y las mascotas nuevas van a la cola FIFO.
/// </summary>
public class OfflinePetRepository : IPetRepository
{
    public const string EntityName = "Mascota";
    private const string CacheKey = "mascotas";
    private const string OfflineMessage =
        "Sin conexión con el servidor: editar y eliminar requieren conexión. Mientras tanto solo puede registrar mascotas nuevas.";
    private const string PendingMessage =
        "Esa mascota todavía está pendiente de enviarse; podrá modificarla cuando se sincronice.";

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
        if (_connectivity.IsOnline)
        {
            try
            {
                var pets = _inner.GetAllPets().ToList();
                TrySaveCache(pets);
                return pets.Concat(GetPendingPets()).ToList();
            }
            catch (Exception) when (!_connectivity.CheckNow())
            {
                // La conexión se perdió justo ahora: se usa la copia local.
            }
        }
        return LoadCache().Concat(GetPendingPets()).ToList();
    }

    public IEnumerable<PetDto> GetAvailablePets()
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                return _inner.GetAvailablePets().ToList();
            }
            catch (Exception) when (!_connectivity.CheckNow())
            {
            }
        }
        return LoadCache().Where(p => p.Status == (int)PetStatus.Disponible).ToList();
    }

    public PetDto? GetPetById(int id)
    {
        if (id < 0) return GetPendingPets().FirstOrDefault(p => p.Id == id);

        if (_connectivity.IsOnline)
        {
            try
            {
                return _inner.GetPetById(id);
            }
            catch (Exception) when (!_connectivity.CheckNow())
            {
            }
        }
        return LoadCache().FirstOrDefault(p => p.Id == id);
    }

    public int CreatePet(PetDto pet)
    {
        if (pet is null) throw new ArgumentNullException(nameof(pet));

        if (_connectivity.IsOnline)
        {
            try
            {
                return _inner.CreatePet(pet);
            }
            catch (Exception) when (!_connectivity.CheckNow())
            {
                // La conexión se perdió justo ahora: la mascota se guarda en la cola local.
            }
        }

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
        if (pet.Id < 0) throw new InvalidOperationException(PendingMessage);
        if (!_connectivity.IsOnline) throw new InvalidOperationException(OfflineMessage);

        try
        {
            _inner.UpdatePet(pet);
        }
        catch (Exception) when (!_connectivity.CheckNow())
        {
            throw new InvalidOperationException(OfflineMessage);
        }
    }

    public void DeletePet(int id)
    {
        if (id < 0) throw new InvalidOperationException(PendingMessage);
        if (!_connectivity.IsOnline) throw new InvalidOperationException(OfflineMessage);

        try
        {
            _inner.DeletePet(id);
        }
        catch (Exception) when (!_connectivity.CheckNow())
        {
            throw new InvalidOperationException(OfflineMessage);
        }
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

    private static void TrySaveCache(List<PetDto> pets)
    {
        try
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
        catch
        {
            // La copia local no debe impedir ver los datos.
        }
    }

    private static List<PetDto> LoadCache()
    {
        using var db = new LocalDbContext();
        var entry = db.Cache.AsNoTracking().FirstOrDefault(c => c.Clave == CacheKey);
        if (entry is null) return new List<PetDto>();
        return JsonSerializer.Deserialize<List<PetDto>>(entry.Json) ?? new List<PetDto>();
    }
}