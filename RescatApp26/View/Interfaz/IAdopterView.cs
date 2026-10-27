using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface IAdopterView
{
    int? SelectedAdopterId { get; }
    string FirstNames { get; }
    string LastNames { get; }
    string DocumentId { get; }
    string? Phone { get; }
    string? Email { get; }
    string? Address { get; }
    string? Country { get; }

    void ShowAdopters(IEnumerable<AdopterDto> adopters);
    void ShowAdopter(AdopterDto adopter);
    void ShowMessage(string message);
    void ShowError(string message);
    bool Confirm(string message);
    void ClearForm();
}