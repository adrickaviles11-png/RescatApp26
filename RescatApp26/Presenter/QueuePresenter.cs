using RescatApp26.Models.DTOs;
using RescatApp26.Models.Local;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class QueuePresenter
{
    private readonly IQueueView _view;
    private readonly ISyncQueue _queue;

    public QueuePresenter(IQueueView view, ISyncQueue queue)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public void Load()
    {
        try
        {
            _view.ShowItems(_queue.GetAll().Select(ToDto).ToList());
            _view.ShowSummary($"Pendientes de envío: {_queue.PendingCount()}. El elemento más antiguo (menor N°) se atiende primero.");
        }
        catch (Exception)
        {
            _view.ShowError("No fue posible leer la cola local.");
        }
    }

    public void EnqueueTest()
    {
        try
        {
            _queue.Enqueue("Prueba", "Crear", $"{{\"mensaje\":\"elemento de prueba {DateTime.Now:HH:mm:ss.fff}\"}}");
            Load();
        }
        catch (Exception)
        {
            _view.ShowError("No fue posible agregar el elemento de prueba.");
        }
    }

    public void ProcessNextTest()
    {
        try
        {
            var next = _queue.PeekNext();
            if (next is null)
            {
                _view.ShowMessage("La cola está vacía.");
                return;
            }

            _queue.MarkAsSent(next.Id);
            _view.ShowMessage($"Se atendió el elemento N° {next.Id}, el más antiguo de la cola.");
            Load();
        }
        catch (Exception)
        {
            _view.ShowError("No fue posible atender el siguiente elemento.");
        }
    }

    private static QueueItemDto ToDto(ColaItem c) => new()
    {
        Id = c.Id,
        Entity = c.Entidad,
        Operation = c.Operacion,
        Status = c.Estado,
        Attempts = c.Intentos,
        CreatedAt = c.FechaCreacion,
        SentAt = c.FechaEnvio,
        LastError = c.UltimoError
    };
}