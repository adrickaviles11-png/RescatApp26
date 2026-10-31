using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface IQueueView
{
    void ShowItems(IEnumerable<QueueItemDto> items);
    void ShowSummary(string text);
    void ShowMessage(string message);
    void ShowError(string message);
}