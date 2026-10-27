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
        _txtUserName = UiBuilder.AddTextBox(this, 30, y, 410, placeholder: "Usuario");
        y += 45;
        _txtPassword = UiBuilder.AddTextBox(this, 30, y, 410, password: true, placeholder: "Contraseña");
        y += 45;
        _txtConfirm = UiBuilder.AddTextBox(this, 30, y, 410, password: true, placeholder: "Confirmar contraseña");
        y += 60;

        var btnRegister = UiBuilder.AddButton(this, "Registrar", 205, y, 115);
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