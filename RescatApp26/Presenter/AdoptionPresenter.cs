using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class AdoptionPresenter
{
    private readonly IAdoptionView _view;
    private readonly IAdoptionRepository _adoptionRepository;
    private readonly IAdopterRepository _adopterRepository;

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
            _view.ShowAdoptions(_adoptionRepository.GetAllAdoptions());
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

    private void ShowFailure(Exception ex, string fallback) =>
        _view.ShowError(ex is InvalidOperationException ? ex.Message : fallback);
}