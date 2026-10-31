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
        ClientSize = new Size(380, 470);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiBuilder.Cream;
        UiBuilder.ApplyIcon(this);

        UiBuilder.AddLogo(this, 135, 20, 110);

        Controls.Add(new Label
        {
            Text = "RescatApp",
            Left = 0,
            Top = 135,
            Width = 380,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = UiBuilder.Primary
        });
        Controls.Add(new Label
        {
            Text = "Rescate y adopción de mascotas",
            Left = 0,
            Top = 178,
            Width = 380,
            Height = 22,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiBuilder.Primary
        });

        _txtUser = UiBuilder.AddTextBox(this, 40, 225, 300, placeholder: "Usuario");
        _txtPassword = UiBuilder.AddTextBox(this, 40, 275, 300, password: true, placeholder: "Contraseña");

        _lblError = new Label { Left = 40, Top = 315, Width = 300, Height = 40, ForeColor = UiBuilder.Danger };
        Controls.Add(_lblError);

        var btnLogin = UiBuilder.AddButton(this, "Iniciar sesión", 40, 365, 300);
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
            Top = 420,
            Width = 300,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            ForeColor = UiBuilder.Primary,
            BackColor = UiBuilder.Cream,
            Cursor = Cursors.Hand
        };
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.Click += (s, e) =>
        {
            try
            {
                using var registerForm = new UserRegisterForm(_service, requestedByAdmin: false);
                registerForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error al abrir el registro");
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