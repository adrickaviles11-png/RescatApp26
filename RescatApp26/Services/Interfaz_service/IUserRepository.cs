using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IUserRepository
{
    UserAuthenticationDataDto? GetUserAuthenticationData(string userName);
    bool ExistsUserName(string userName);
    bool AnyUsers();
    int CreateUser(UserCreateDto user);
    UserRecoveryDataDto? GetRecoveryData(string userName);
    void UpdatePasswordHash(int userId, string passwordHash);
}