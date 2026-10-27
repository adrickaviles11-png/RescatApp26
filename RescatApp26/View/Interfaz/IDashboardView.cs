using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface IDashboardView
{
    void ShowSummary(DashboardDto summary);
    void ShowError(string message);
}