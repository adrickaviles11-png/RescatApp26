using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class UserRegisterForm : Form, IUserRegisterView
{
    private readonly UserRegisterPresenter _presenter;
    private readonly TextBox _txtUserName;
    private readonly TextBox _txtPassword;
    private readonly TextBox _txtConfirm;

    public UserRegisterForm(IAuthenticationService service)
    {
        _presenter = new UserRegisterPresenter(this, service);

        Text = "Registro de usuario";
        ClientSize = new Size(480, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        UiBuilder.AddTitle(this, "Registro de usuario", 30, 20);
        UiBuilder.AddLabel(this, "Complete los datos para crear su cuenta.", 30, 58);

        int y = 100;
        UiBuilder.AddLabel(this, "Usuario", 30, y);
        _txtUserName = UiBuilder.AddTextBox(this, 200, y, 240);
        y += 42;
        UiBuilder.AddLabel(this, "Contraseña", 30, y);
        _txtPassword = UiBuilder.AddTextBox(this, 200, y, 240, password: true);
        y += 42;
        UiBuilder.AddLabel(this, "Confirmar contraseña", 30, y);
        _txtConfirm = UiBuilder.AddTextBox(this, 200, y, 240, password: true);
        y += 60;

        var btnRegister = UiBuilder.AddButton(this, "Registrar", 200, y, 115);
        btnRegister.Click += (s, e) => _presenter.Register();
        var btnClose = UiBuilder.AddButton(this, "Cerrar", 325, y, 115, danger: true);
        btnClose.Click += (s, e) => Close();
        AcceptButton = btnRegister;
    }

    public string UserName => _txtUserName.Text.Trim();
    public string Password => _txtPassword.Text;
    public string ConfirmPassword => _txtConfirm.Text;

    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);

    public void ClearForm()
    {
        _txtUserName.Clear();
        _txtPassword.Clear();
        _txtConfirm.Clear();
        _txtUserName.Focus();
    }

    public void CloseView() => Close();
}