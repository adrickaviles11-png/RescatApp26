using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IPetRepository
{
    IEnumerable<PetDto> GetAllPets();
    IEnumerable<PetDto> GetAvailablePets();
    PetDto? GetPetById(int id);
    int CreatePet(PetDto pet);
    void UpdatePet(PetDto pet);
    void DeletePet(int id);
}