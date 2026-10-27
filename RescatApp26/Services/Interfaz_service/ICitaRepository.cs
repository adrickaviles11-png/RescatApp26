using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface ICitaRepository
{
    IEnumerable<CitaDto> GetAllCitas();
    CitaDto? GetCitaById(int id);
    IEnumerable<LookupItemDto> GetPetsForCitas();
    IEnumerable<LookupItemDto> GetVeterinarias();
    int CreateCita(CitaDto cita);
    void UpdateCita(CitaDto cita);
    void CancelCita(int id);
}