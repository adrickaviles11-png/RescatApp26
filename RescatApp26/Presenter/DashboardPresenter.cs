using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class DashboardPresenter
{
    private readonly IDashboardView _view;
    private readonly IDashboardRepository _repository;

    public DashboardPresenter(IDashboardView view, IDashboardRepository repository)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public void LoadSummary()
    {
        try
        {
            _view.ShowSummary(_repository.GetSummary(DateTime.Today));
        }
        catch (Exception ex)
        {
            _view.ShowError(ex is InvalidOperationException ? ex.Message : "No fue posible cargar el resumen.");
        }
    }
}