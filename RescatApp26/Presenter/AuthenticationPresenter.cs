using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class AuthenticationPresenter
{
    private readonly IAuthenticationView _view;
    private readonly IAuthenticationService _service;

    public AuthenticationPresenter(IAuthenticationView view, IAuthenticationService service)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Authenticate()
    {
        try
        {
            var result = _service.Authenticate(new AuthenticationRequestDto
            {
                UserName = _view.UserName,
                Password = _view.Password
            });

            if (result.IsAuthenticated && result.User is not null)
                _view.ShowAuthenticatedUser(result.User);
            else
                _view.ShowAuthenticationError(result.Message);
        }
        catch (InvalidOperationException ex)
        {
            _view.ShowAuthenticationError(ex.Message);
        }
        catch (Exception)
        {
            _view.ShowAuthenticationError("No fue posible iniciar sesión.");
        }
    }
}