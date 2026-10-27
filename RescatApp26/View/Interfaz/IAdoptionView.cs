using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface IAdoptionView
{
    int? SelectedPetId { get; }
    int? SelectedAdopterId { get; }
    decimal Cost { get; }
    string? Notes { get; }

    void ShowAdoptablePets(IEnumerable<LookupItemDto> pets);
    void ShowAdopters(IEnumerable<AdopterDto> adopters);
    void ShowAdoptions(IEnumerable<AdoptionListDto> adoptions);
    void ShowMessage(string message);
    void ShowError(string message);
    void ClearForm();
}