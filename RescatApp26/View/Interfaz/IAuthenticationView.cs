using RescatApp26.Models.DTOs;

namespace RescatApp26.View.Interfaz;

public interface IAuthenticationView
{
    string UserName { get; }
    string Password { get; }
    void ShowAuthenticationError(string message);
    void ShowAuthenticatedUser(AuthenticatedUserDto user);
}