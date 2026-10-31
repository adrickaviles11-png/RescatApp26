using RescatApp26.Models.Enums;
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
    private readonly ComboBox _cboRole;

    public UserRegisterForm(IAuthenticationService service, bool requestedByAdmin)
    {
        _presenter = new UserRegisterPresenter(this, service, requestedByAdmin);

        Text = "Registro de usuario";
        ClientSize = new Size(480, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        UiBuilder.AddTitle(this, "Registro de usuario", 30, 20);
        UiBuilder.AddLabel(this, requestedByAdmin
            ? "Cree una cuenta para el equipo o para un adoptante."
            : "Complete los datos para crear su cuenta de adoptante.", 30, 58);

        int y = 100;
        _txtUserName = UiBuilder.AddTextBox(this, 30, y, 410, placeholder: "Usuario");
        y += 45;
        _txtPassword = UiBuilder.AddTextBox(this, 30, y, 410, password: true, placeholder: "Contraseña");
        y += 45;
        _txtConfirm = UiBuilder.AddTextBox(this, 30, y, 410, password: true, placeholder: "Confirmar contraseña");
        y += 45;

        UiBuilder.AddLabel(this, "Rol", 30, y);
        _cboRole = new ComboBox
        {
            Left = 140,
            Top = y,
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Text",
            ValueMember = "Value"
        };
        Controls.Add(_cboRole);
        y += 55;

        var btnRegister = UiBuilder.AddButton(this, "Registrar", 205, y, 115);
        btnRegister.Click += (s, e) => _presenter.Register();
        var btnClose = UiBuilder.AddButton(this, "Cerrar", 325, y, 115, danger: true);
        btnClose.Click += (s, e) => Close();
        AcceptButton = btnRegister;

        Load += (s, e) => _presenter.LoadRoles();
    }

    public string UserName => _txtUserName.Text.Trim();
    public string Password => _txtPassword.Text;
    public string ConfirmPassword => _txtConfirm.Text;

    // Si no hay nada seleccionado devuelve -1 (inválido), nunca 0, que es el rol de Administrador.
    public int RoleCode => _cboRole.SelectedValue is int v ? v : -1;

    public void SetAvailableRoles(IEnumerable<OptionItem> roles) => _cboRole.DataSource = roles.ToList();

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