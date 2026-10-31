using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;

namespace RescatApp26.Services.Interfaz_service;

public interface IAuthenticationService
{
    AuthenticationResultDto Authenticate(AuthenticationRequestDto request);
    OperationResultDto Register(UserRegisterDto request, bool requestedByAdmin);
    IReadOnlyList<OptionItem> GetAvailableRoles(bool requestedByAdmin);
}