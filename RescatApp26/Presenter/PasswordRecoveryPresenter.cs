using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.Presenter;

public class PasswordRecoveryPresenter
{
    private const int MaxFailedAttempts = 3;

    private readonly IPasswordRecoveryView _view;
    private readonly IAuthenticationService _service;
    private int _failedAttempts;

    public PasswordRecoveryPresenter(IPasswordRecoveryView view, IAuthenticationService service)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void FindAccount()
    {
        try
        {
            string? question = _service.GetSecurityQuestion(_view.UserName);
            if (question is null)
            {
                _view.ShowError("No fue posible iniciar la recuperación para ese usuario. " +
                                "Verifique el nombre o pida ayuda a un administrador.");
                return;
            }
            _view.ShowQuestion(question);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex is InvalidOperationException ? ex.Message : "No fue posible buscar la cuenta.");
        }
    }

    public void ResetPassword()
    {
        try
        {
            var result = _service.ResetPassword(_view.UserName, _view.Answer, _view.NewPassword, _view.ConfirmPassword);
            if (result.Success)
            {
                _view.ShowMessage(result.Message);
                _view.CloseView();
                return;
            }

            _failedAttempts++;
            if (_failedAttempts >= MaxFailedAttempts)
            {
                _view.ShowError("Demasiados intentos fallidos. Cierre esta ventana e inténtelo de nuevo más tarde.");
                _view.CloseView();
                return;
            }

            _view.ShowError($"{result.Message} (intento {_failedAttempts} de {MaxFailedAttempts})");
        }
        catch (Exception ex)
        {
            _view.ShowError(ex is InvalidOperationException ? ex.Message : "No fue posible restablecer la contraseña.");
        }
    }
}