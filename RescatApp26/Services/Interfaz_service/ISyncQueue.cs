using RescatApp26.Models.Local;

namespace RescatApp26.Services.Interfaz_service;

/// <summary>Cola FIFO persistente: el primero que entra es el primero que sale.</summary>
public interface ISyncQueue
{
    long Enqueue(string entity, string operation, string payload);   // entra al final
    ColaItem? PeekNext();                                            // el más antiguo pendiente (el frente)
    void MarkAsSent(long id);                                        // sale de la cola
    void MarkAsFailed(long id, string error, int maxAttempts);
    int PendingCount();
    IReadOnlyList<ColaItem> GetAll();
}