using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class CitaPresenter
{
    private readonly ICitaView _view;
    private readonly ICitaRepository _citaRepository;
    private readonly IAdopterRepository _adopterRepository;

    public CitaPresenter(ICitaView view, ICitaRepository citaRepository, IAdopterRepository adopterRepository)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
        _adopterRepository = adopterRepository ?? throw new ArgumentNullException(nameof(adopterRepository));
    }

    public void LoadData()
    {
        try
        {
            _view.ShowPets(_citaRepository.GetPetsForCitas());
            _view.ShowVeterinarias(_citaRepository.GetVeterinarias());
            _view.ShowAdopters(_adopterRepository.GetAllAdopters());
            _view.ShowCitas(_citaRepository.GetAllCitas());
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible cargar los datos de citas.");
        }
    }

    private void LoadCitas()
    {
        _view.ShowCitas(_citaRepository.GetAllCitas());
    }

    public void SelectCita(int citaId)
    {
        try
        {
            var cita = _citaRepository.GetCitaById(citaId);
            if (cita is null) _view.ShowError("La cita seleccionada ya no existe.");
            else _view.ShowCita(cita);
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible obtener la cita.");
        }
    }

    public void NewCita() => _view.ClearForm();

    public void SaveCita()
    {
        try
        {
            if (_view.SelectedPetId is null)
            {
                _view.ShowError("Seleccione una mascota.");
                return;
            }

            bool isVet = _view.TypeCode == (int)CitaTipo.Veterinaria;
            if (isVet && _view.SelectedVeterinaryId is null)
            {
                _view.ShowError("Seleccione la veterinaria.");
                return;
            }
            if (!isVet && _view.SelectedAdopterId is null)
            {
                _view.ShowError("Seleccione el adoptante. Si no existe, regístrelo primero en Adoptantes.");
                return;
            }

            var dto = new CitaDto
            {
                Id = _view.SelectedCitaId ?? 0,
                Type = _view.TypeCode,
                PetId = _view.SelectedPetId.Value,
                VeterinaryId = isVet ? _view.SelectedVeterinaryId : null,
                AdopterId = isVet ? null : _view.SelectedAdopterId,
                ScheduledAt = _view.ScheduledAt,
                Reason = _view.Reason,
                Status = _view.StatusCode,
                Notes = _view.Notes
            };

            if (_view.SelectedCitaId is null)
            {
                _citaRepository.CreateCita(dto);
                _view.ShowMessage("Cita registrada correctamente.");
            }
            else
            {
                _citaRepository.UpdateCita(dto);
                _view.ShowMessage("Cita actualizada correctamente.");
            }

            LoadCitas();
            _view.ClearForm();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible guardar la cita.");
        }
    }

    public void CancelCita()
    {
        try
        {
            if (_view.SelectedCitaId is null)
            {
                _view.ShowError("Seleccione una cita de la lista.");
                return;
            }

            if (!_view.Confirm("¿Desea cancelar la cita seleccionada?")) return;

            _citaRepository.CancelCita(_view.SelectedCitaId.Value);
            _view.ShowMessage("Cita cancelada correctamente.");
            LoadCitas();
            _view.ClearForm();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible cancelar la cita.");
        }
    }

    private void ShowFailure(Exception ex, string fallback) =>
        _view.ShowError(ex is InvalidOperationException ? ex.Message : fallback);
}