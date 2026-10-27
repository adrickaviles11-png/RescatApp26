using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IAdopterRepository
{
    IEnumerable<AdopterDto> GetAllAdopters();
    AdopterDto? GetAdopterById(int id);
    int CreateAdopter(AdopterDto adopter);
    void UpdateAdopter(AdopterDto adopter);
    void DeleteAdopter(int id);
}