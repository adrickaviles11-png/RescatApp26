using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.Local;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

/// <summary>
/// Cola FIFO guardada en SQLite. El orden lo da el Id AUTOINCREMENT: nunca se reutiliza ni retrocede,
/// así que "el más antiguo" siempre es el de menor Id pendiente.
/// Cada método usa su propio contexto, por lo que se puede llamar desde varios hilos.
/// </summary>
public class SqliteSyncQueue : ISyncQueue
{
    public long Enqueue(string entity, string operation, string payload)
    {
        using var db = new LocalDbContext();
        var item = new ColaItem
        {
            Entidad = entity,
            Operacion = operation,
            Payload = payload,
            FechaCreacion = DateTime.Now
        };
        db.Cola.Add(item);
        db.SaveChanges();
        return item.Id;
    }

    public ColaItem? PeekNext()
    {
        using var db = new LocalDbContext();
        return db.Cola.AsNoTracking()
            .Where(c => c.Estado == QueueStates.Pendiente)
            .OrderBy(c => c.Id)
            .FirstOrDefault();
    }

    public void MarkAsSent(long id)
    {
        using var db = new LocalDbContext();
        var item = db.Cola.FirstOrDefault(c => c.Id == id);
        if (item is null) return;

        item.Estado = QueueStates.Enviado;
        item.FechaEnvio = DateTime.Now;
        item.UltimoError = null;
        db.SaveChanges();
    }

    public void MarkAsFailed(long id, string error, int maxAttempts)
    {
        using var db = new LocalDbContext();
        var item = db.Cola.FirstOrDefault(c => c.Id == id);
        if (item is null) return;

        item.Intentos++;
        item.UltimoError = error.Length <= 500 ? error : error[..500];
        if (item.Intentos >= maxAttempts) item.Estado = QueueStates.Error;
        db.SaveChanges();
    }

    public int PendingCount()
    {
        using var db = new LocalDbContext();
        return db.Cola.Count(c => c.Estado == QueueStates.Pendiente);
    }

    public IReadOnlyList<ColaItem> GetAll()
    {
        using var db = new LocalDbContext();
        return db.Cola.AsNoTracking().OrderBy(c => c.Id).ToList();
    }
}