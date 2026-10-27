using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IAdoptionRepository
{
    IEnumerable<LookupItemDto> GetAdoptablePets();
    IEnumerable<AdoptionListDto> GetAllAdoptions();

    /// <summary>Registra la adopción, marca la mascota como Adoptada y devuelve el código asignado por la secuencia.</summary>
    int CreateAdoption(AdoptionCreateDto adoption);
}