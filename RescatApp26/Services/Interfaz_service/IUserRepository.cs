using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IUserRepository
{
    UserAuthenticationDataDto? GetUserAuthenticationData(string userName);
    bool ExistsUserName(string userName);
    int CreateUser(UserCreateDto user);
    bool AnyUsers();
}