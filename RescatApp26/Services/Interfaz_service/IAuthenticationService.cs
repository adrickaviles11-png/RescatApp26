using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IAuthenticationService
{
    AuthenticationResultDto Authenticate(AuthenticationRequestDto request);
    OperationResultDto Register(UserRegisterDto request);
}