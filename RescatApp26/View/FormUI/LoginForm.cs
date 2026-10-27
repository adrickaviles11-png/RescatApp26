using RescatApp26.Models.DTOs;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class LoginForm : Form, IAuthenticationView
{
    private readonly AuthenticationPresenter _presenter;
    private readonly IAuthenticationService _service;
    private readonly TextBox _txtUser;
    private readonly TextBox _txtPassword;
    private readonly Label _lblError;

    public AuthenticatedUserDto? AuthenticatedUser { get; private set; }

    public LoginForm(IAuthenticationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _presenter = new AuthenticationPresenter(this, service);

        Text = "RescatApp - Iniciar sesión";
        ClientSize = new Size(380, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        var title = new Label
        {
            Text = "RescatApp",
            Left = 0,
            Top = 25,
            Width = 380,
            Height = 45,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = UiBuilder.Primary
        };
        var subtitle = new Label
        {
            Text = "Rescate y adopción de mascotas",
            Left = 0,
            Top = 72,
            Width = 380,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Gray
        };
        Controls.Add(title);
        Controls.Add(subtitle);

        UiBuilder.AddLabel(this, "Usuario", 40, 115);
        _txtUser = UiBuilder.AddTextBox(this, 40, 140, 300);
        UiBuilder.AddLabel(this, "Contraseña", 40, 180);
        _txtPassword = UiBuilder.AddTextBox(this, 40, 205, 300, password: true);

        _lblError = new Label { Left = 40, Top = 240, Width = 300, Height = 36, ForeColor = UiBuilder.Danger };
        Controls.Add(_lblError);

        var btnLogin = UiBuilder.AddButton(this, "Iniciar sesión", 40, 285, 300);
        btnLogin.Height = 40;
        btnLogin.Click += (s, e) =>
        {
            _lblError.Text = string.Empty;
            _presenter.Authenticate();
        };
        AcceptButton = btnLogin;

        var btnRegister = new Button
        {
            Text = "¿No tiene cuenta? Crear cuenta nueva",
            Left = 40,
            Top = 335,
            Width = 300,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            ForeColor = UiBuilder.Primary,
            Cursor = Cursors.Hand
        };
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.Click += (s, e) =>
        {
            try
            {
                using var registerForm = new UserRegisterForm(_service);
                registerForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _lblError.Text = ex is InvalidOperationException ? ex.Message : "No fue posible abrir el registro.";
            }
        };
        Controls.Add(btnRegister);
    }

    public string UserName => _txtUser.Text.Trim();
    public string Password => _txtPassword.Text;

    public void ShowAuthenticationError(string message)
    {
        _lblError.Text = message;
        _txtPassword.Clear();
        _txtPassword.Focus();
    }

    public void ShowAuthenticatedUser(AuthenticatedUserDto user)
    {
        AuthenticatedUser = user;
        DialogResult = DialogResult.OK;
    }
}