using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class UserRegisterPresenter
{
    private readonly IUserRegisterView _view;
    private readonly IAuthenticationService _service;

    public UserRegisterPresenter(IUserRegisterView view, IAuthenticationService service)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Register()
    {
        try
        {
            var result = _service.Register(new UserRegisterDto
            {
                UserName = _view.UserName,
                Password = _view.Password,
                ConfirmPassword = _view.ConfirmPassword
            });

            if (!result.Success)
            {
                _view.ShowError(result.Message);
                return;
            }

            _view.ShowMessage(result.Message);
            _view.ClearForm();
            _view.CloseView();
        }
        catch (Exception ex)
        {
            _view.ShowError(ex is InvalidOperationException ? ex.Message : "No fue posible registrar el usuario.");
        }
    }
}