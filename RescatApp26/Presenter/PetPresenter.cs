using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class PetPresenter
{
    private readonly IPetView _view;
    private readonly IPetRepository _repository;
    private List<PetDto> _all = new();
    private string _filterText = string.Empty;
    private int? _filterStatus;

    public PetPresenter(IPetView view, IPetRepository repository)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public void LoadPets()
    {
        try
        {
            _all = _repository.GetAllPets().ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible cargar las mascotas.");
        }
    }

    public void SetFilter(string text, int? status)
    {
        _filterText = text ?? string.Empty;
        _filterStatus = status;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        string text = _filterText.Trim();
        var query = _all.AsEnumerable();

        if (_filterStatus is int status)
            query = query.Where(p => p.Status == status);

        if (text.Length > 0)
            query = query.Where(p => FilterHelper.Contains(p.Name, text) || FilterHelper.Contains(p.Species, text)
                || FilterHelper.Contains(p.Breed, text) || FilterHelper.Contains(p.Description, text));

        _view.ShowPets(query.ToList());
    }

    public void SelectPet(int petId)
    {
        try
        {
            var pet = _repository.GetPetById(petId);
            if (pet is null) _view.ShowError("La mascota seleccionada ya no existe.");
            else _view.ShowPet(pet);
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible obtener la mascota.");
        }
    }

    public void NewPet() => _view.ClearForm();

    public void SavePet()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_view.PetName))
            {
                _view.ShowError("El nombre de la mascota es obligatorio.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_view.Species))
            {
                _view.ShowError("La especie es obligatoria.");
                return;
            }

            var dto = new PetDto
            {
                Id = _view.SelectedPetId ?? 0,
                Name = _view.PetName.Trim(),
                Species = _view.Species.Trim(),
                Breed = _view.Breed,
                AgeMonths = _view.AgeMonths,
                Sex = _view.SexCode,
                Size = _view.SizeCode,
                Description = _view.Description,
                Status = _view.StatusCode
            };

            if (_view.SelectedPetId is null)
            {
                int newId = _repository.CreatePet(dto);
                _view.ShowMessage(newId < 0
                    ? "Sin conexión: la mascota se guardó en la cola local y se enviará al servidor cuando vuelva la conexión."
                    : "Mascota registrada correctamente.");
            }
            else
            {
                _repository.UpdatePet(dto);
                _view.ShowMessage("Mascota actualizada correctamente.");
            }

            LoadPets();
            _view.ClearForm();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible guardar la mascota.");
        }
    }

    public void DeletePet()
    {
        try
        {
            if (_view.SelectedPetId is null)
            {
                _view.ShowError("Seleccione una mascota de la lista.");
                return;
            }

            if (!_view.Confirm("¿Desea eliminar la mascota seleccionada?")) return;

            _repository.DeletePet(_view.SelectedPetId.Value);
            _view.ShowMessage("Mascota eliminada correctamente.");
            LoadPets();
            _view.ClearForm();
        }
        catch (Exception ex)
        {
            ShowFailure(ex, "No fue posible eliminar la mascota.");
        }
    }

    // Si el error es InvalidOperationException su mensaje ya es legible; si no, se muestra uno genérico.
    private void ShowFailure(Exception ex, string fallback) =>
        _view.ShowError(ex is InvalidOperationException ? ex.Message : fallback);
}