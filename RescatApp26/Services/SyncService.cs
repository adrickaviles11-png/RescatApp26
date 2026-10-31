using System.Text.Json;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Local;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;

namespace RescatApp26.Services;

/// <summary>Vacía la cola FIFO hacia SQL Server: apenas vuelve la conexión, y cada 5 segundos como respaldo.</summary>
public class SyncService : IDisposable
{
    private const int MaxAttempts = 3;

    private readonly ISyncQueue _queue;
    private readonly IConnectivityService _connectivity;
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _gate = new(1, 1);   // evita dos envíos al mismo tiempo (duplicados)

    /// <summary>Se dispara cada vez que un elemento sale de la cola o falla.</summary>
    public event EventHandler? QueueChanged;

    public SyncService(ISyncQueue queue, IConnectivityService connectivity)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
    }

    public void Start()
    {
        // Apenas vuelve la conexión se envía lo pendiente, sin esperar al siguiente ciclo.
        _connectivity.StatusChanged += (s, online) =>
        {
            if (online) _ = Task.Run(() => ProcessQueueAsync(_cts.Token));
        };
        _ = Task.Run(() => RunAsync(_cts.Token));
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
                await ProcessQueueAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // La aplicación se está cerrando.
        }
    }

    public async Task<int> ProcessQueueAsync(CancellationToken ct = default)
    {
        if (!await _gate.WaitAsync(0, ct)) return 0;   // ya hay un envío en curso

        int sent = 0;
        try
        {
            if (!_connectivity.IsOnline) return 0;

            while (!ct.IsCancellationRequested)
            {
                var item = _queue.PeekNext();   // FIFO: siempre el más antiguo pendiente
                if (item is null) break;

                try
                {
                    await Task.Run(() => Send(item), ct);
                    _queue.MarkAsSent(item.Id);
                    sent++;
                    QueueChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Si se cayó la conexión, el elemento no se penaliza: se reintenta al volver.
                    if (!_connectivity.CheckNow()) break;

                    _queue.MarkAsFailed(item.Id, ex.InnerException?.Message ?? ex.Message, MaxAttempts);
                    QueueChanged?.Invoke(this, EventArgs.Empty);
                    break;   // se detiene para respetar el orden; reintenta en el siguiente ciclo
                }
            }
        }
        catch (OperationCanceledException)
        {
            // La aplicación se está cerrando.
        }
        finally
        {
            _gate.Release();
        }
        return sent;
    }

    private static void Send(ColaItem item)
    {
        switch (item.Entidad)
        {
            case OfflinePetRepository.EntityName when item.Operacion == "Crear":
                var pet = JsonSerializer.Deserialize<PetDto>(item.Payload)
                    ?? throw new InvalidOperationException("Los datos de la mascota no son válidos.");
                // Contexto propio: un DbContext no se comparte entre hilos.
                using (var db = new RescatAppDbContext())
                    new PetRepository(db).CreatePet(pet);
                break;

            case "Prueba":
                break;   // elementos de prueba de la pantalla de la cola

            default:
                throw new InvalidOperationException($"Operación no soportada: {item.Entidad} / {item.Operacion}");
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _gate.Dispose();
    }
}