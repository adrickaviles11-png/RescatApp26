using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class AdoptionPresenter
{
    private readonly IAdoptionView _view;
    private readonly IAdoptionRepository _adoptionRepository;
    private readonly IAdopterRepository _adopterRepository;
    private List<AdoptionListDto> _allAdoptions = new();
    private string _filterText = string.Empty;
    private DateTime? _filterFrom;
    private DateTime? _filterTo;

    public AdoptionPresenter(IAdoptionView view, IAdoptionRepository adoptionRepository, IAdopterRepository adopterRepository)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _adoptionRepository = adoptionRepository ?? throw new ArgumentNullException(nameof(adoptionRepository));
        _adopterRepository = adopterRepository ?? throw new ArgumentNullException(nameof(adopterRepository));
    }

    public void LoadData()
    {
        try
        {
            _view.ShowAdoptablePets(_adoptionRepository.GetAdoptablePets());
            _view.ShowAdopters(_adopterRepository.GetAllAdopters());
            _allAdoptions = _adoptionRepository.GetAllAdoptions().ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible cargar los datos de adopciones.");
        }
    }

    public void RegisterAdoption()
    {
        try
        {
            if (_view.SelectedPetId is null)
            {
                _view.ShowError("No hay una mascota disponible para adoptar.");
                return;
            }
            if (_view.SelectedAdopterId is null)
            {
                _view.ShowError("Seleccione un adoptante. Si no existe, regístrelo primero en la sección Adoptantes.");
                return;
            }

            int code = _adoptionRepository.CreateAdoption(new AdoptionCreateDto
            {
                PetId = _view.SelectedPetId.Value,
                AdopterId = _view.SelectedAdopterId.Value,
                Cost = _view.Cost,
                Notes = _view.Notes
            });

            _view.ShowMessage($"Adopción registrada correctamente.\nCódigo de adopción: {code}");
            _view.ClearForm();
            LoadData();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible registrar la adopción.");
        }
    }
    public void SetFilter(string text, DateTime? from, DateTime? to)
    {
        _filterText = text ?? string.Empty;
        _filterFrom = from;
        _filterTo = to;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        string text = _filterText.Trim();
        var query = _allAdoptions.AsEnumerable();

        if (_filterFrom is DateTime fromDate) query = query.Where(a => a.Date.Date >= fromDate.Date);
        if (_filterTo is DateTime toDate) query = query.Where(a => a.Date.Date <= toDate.Date);

        if (text.Length > 0)
            query = query.Where(a => FilterHelper.Contains(a.PetName, text) || FilterHelper.Contains(a.AdopterName, text)
                || FilterHelper.Contains(a.Code.ToString(), text) || FilterHelper.Contains(a.Notes, text));

        _view.ShowAdoptions(query.ToList());
    }
    private void ShowFailure(Exception ex, string fallback) =>
        _view.ShowError(ex is InvalidOperationException ? ex.Message : fallback);
}