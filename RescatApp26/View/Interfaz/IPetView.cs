using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface IPetView
{
    int? SelectedPetId { get; }
    string PetName { get; }
    string Species { get; }
    string? Breed { get; }
    int AgeMonths { get; }
    int SexCode { get; }
    int SizeCode { get; }
    string? Description { get; }
    int StatusCode { get; }

    void ShowPets(IEnumerable<PetDto> pets);
    void ShowPet(PetDto pet);
    void ShowMessage(string message);
    void ShowError(string message);
    bool Confirm(string message);
    void ClearForm();
}