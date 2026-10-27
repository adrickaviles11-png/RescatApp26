using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface ICitaView
{
    int? SelectedCitaId { get; }
    int TypeCode { get; }
    int? SelectedPetId { get; }
    int? SelectedVeterinaryId { get; }
    int? SelectedAdopterId { get; }
    DateTime ScheduledAt { get; }
    string? Reason { get; }
    int StatusCode { get; }
    string? Notes { get; }

    void ShowCitas(IEnumerable<CitaDto> citas);
    void ShowCita(CitaDto cita);
    void ShowPets(IEnumerable<LookupItemDto> pets);
    void ShowVeterinarias(IEnumerable<LookupItemDto> veterinarias);
    void ShowAdopters(IEnumerable<AdopterDto> adopters);
    void ShowMessage(string message);
    void ShowError(string message);
    bool Confirm(string message);
    void ClearForm();
}