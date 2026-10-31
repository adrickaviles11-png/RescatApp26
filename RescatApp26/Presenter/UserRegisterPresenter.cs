using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class UserRegisterPresenter
{
    private readonly IUserRegisterView _view;
    private readonly IAuthenticationService _service;
    private readonly bool _requestedByAdmin;

    public UserRegisterPresenter(IUserRegisterView view, IAuthenticationService service, bool requestedByAdmin)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _requestedByAdmin = requestedByAdmin;
    }

    public void LoadRoles()
    {
        try
        {
            _view.SetAvailableRoles(_service.GetAvailableRoles(_requestedByAdmin));
        }
        catch (Exception ex)
        {
            _view.ShowError(ex is InvalidOperationException ? ex.Message : "No fue posible cargar los roles.");
        }
    }

    public void Register()
    {
        try
        {
            var result = _service.Register(new UserRegisterDto
            {
                UserName = _view.UserName,
                Password = _view.Password,
                ConfirmPassword = _view.ConfirmPassword,
                Role = _view.RoleCode
            }, _requestedByAdmin);

            if (!result.Success)
            {
                _view.ShowError(result.Message);
                return;
            }

            _view.ShowMessage(result.Message);
            _view.ClearForm();
            LoadRoles();

            // Desde el inicio de sesión se cierra al terminar; el administrador puede seguir creando usuarios.
            if (!_requestedByAdmin) _view.CloseView();
        }
        catch (Exception ex)
        {
            _view.ShowError(ex is InvalidOperationException ? ex.Message : "No fue posible registrar el usuario.");
        }
    }
}