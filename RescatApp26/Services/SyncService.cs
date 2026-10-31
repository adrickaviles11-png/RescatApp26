using System.Text.Json;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Local;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;

namespace RescatApp26.Services;

/// <summary>Vacía la cola FIFO hacia SQL Server cada 5 segundos, mientras haya conexión.</summary>
public class SyncService : IDisposable
{
    private const int MaxAttempts = 3;

    private readonly ISyncQueue _queue;
    private readonly IConnectivityService _connectivity;
    private readonly CancellationTokenSource _cts = new();

    public SyncService(ISyncQueue queue, IConnectivityService connectivity)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
    }

    public void Start() => _ = Task.Run(() => RunAsync(_cts.Token));

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
        int sent = 0;
        if (!await Task.Run(() => _connectivity.IsOnline(), ct)) return 0;

        while (!ct.IsCancellationRequested)
        {
            var item = _queue.PeekNext();   // FIFO: siempre el más antiguo pendiente
            if (item is null) break;

            try
            {
                await Task.Run(() => Send(item), ct);
                _queue.MarkAsSent(item.Id);
                sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Si el fallo fue por pérdida de conexión, no cuenta como intento fallido.
                if (!_connectivity.IsOnline()) break;

                _queue.MarkAsFailed(item.Id, ex.Message, MaxAttempts);
                break;   // se detiene para respetar el orden; reintenta en el siguiente ciclo
            }
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
    }
}