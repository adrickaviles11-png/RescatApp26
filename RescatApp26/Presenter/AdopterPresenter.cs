using System.Net.Mail;
using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class AdopterPresenter
{
    private readonly IAdopterView _view;
    private readonly IAdopterRepository _repository;

    public AdopterPresenter(IAdopterView view, IAdopterRepository repository)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public void LoadAdopters()
    {
        try
        {
            _view.ShowAdopters(_repository.GetAllAdopters());
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible cargar los adoptantes.");
        }
    }

    public void SelectAdopter(int adopterId)
    {
        try
        {
            var adopter = _repository.GetAdopterById(adopterId);
            if (adopter is null) _view.ShowError("El adoptante seleccionado ya no existe.");
            else _view.ShowAdopter(adopter);
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible obtener el adoptante.");
        }
    }

    public void NewAdopter() => _view.ClearForm();

    public void SaveAdopter()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_view.FirstNames) || string.IsNullOrWhiteSpace(_view.LastNames))
            {
                _view.ShowError("Nombres y apellidos son obligatorios.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_view.DocumentId))
            {
                _view.ShowError("El documento de identidad es obligatorio.");
                return;
            }
            if (_view.Email is not null && !MailAddress.TryCreate(_view.Email, out _))
            {
                _view.ShowError("El correo electrónico no es válido.");
                return;
            }

            var dto = new AdopterDto
            {
                Id = _view.SelectedAdopterId ?? 0,
                FirstNames = _view.FirstNames.Trim(),
                LastNames = _view.LastNames.Trim(),
                DocumentId = _view.DocumentId.Trim(),
                Phone = _view.Phone,
                Email = _view.Email,
                Address = _view.Address,
                Country = _view.Country
            };

            if (_view.SelectedAdopterId is null)
            {
                _repository.CreateAdopter(dto);
                _view.ShowMessage("Adoptante registrado correctamente.");
            }
            else
            {
                _repository.UpdateAdopter(dto);
                _view.ShowMessage("Adoptante actualizado correctamente.");
            }

            LoadAdopters();
            _view.ClearForm();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible guardar el adoptante.");
        }
    }

    public void DeleteAdopter()
    {
        try
        {
            if (_view.SelectedAdopterId is null)
            {
                _view.ShowError("Seleccione un adoptante de la lista.");
                return;
            }

            if (!_view.Confirm("¿Desea eliminar el adoptante seleccionado?")) return;

            _repository.DeleteAdopter(_view.SelectedAdopterId.Value);
            _view.ShowMessage("Adoptante eliminado correctamente.");
            LoadAdopters();
            _view.ClearForm();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible eliminar el adoptante.");
        }
    }

    private void ShowFailure(Exception ex, string fallback) =>
        _view.ShowError(ex is InvalidOperationException ? ex.Message : fallback);
}