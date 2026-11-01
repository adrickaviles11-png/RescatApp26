using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class PasswordRecoveryForm : Form, IPasswordRecoveryView
{
    private readonly PasswordRecoveryPresenter _presenter;
    private readonly TextBox _txtUserName;
    private readonly Button _btnFind;
    private readonly Label _lblQuestion;
    private readonly TextBox _txtAnswer;
    private readonly TextBox _txtNewPassword;
    private readonly TextBox _txtConfirm;
    private readonly Button _btnReset;

    public PasswordRecoveryForm(IAuthenticationService service)
    {
        _presenter = new PasswordRecoveryPresenter(this, service);

        Text = "Recuperar contraseña";
        ClientSize = new Size(470, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        UiBuilder.AddTitle(this, "Recuperar contraseña", 30, 20);
        UiBuilder.AddLabel(this, "Escriba su usuario para ver su pregunta de seguridad.", 30, 58);

        _txtUserName = UiBuilder.AddTextBox(this, 30, 95, 300, placeholder: "Usuario");
        _btnFind = UiBuilder.AddButton(this, "Buscar", 345, 93, 95);
        _btnFind.Click += (s, e) => _presenter.FindAccount();
        AcceptButton = _btnFind;

        // Segundo paso: aparece cuando se encuentra la cuenta.
        _lblQuestion = new Label
        {
            Left = 30,
            Top = 145,
            Width = 410,
            Height = 40,
            Visible = false,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UiBuilder.Primary
        };
        Controls.Add(_lblQuestion);
        _txtAnswer = UiBuilder.AddTextBox(this, 30, 195, 410, placeholder: "Respuesta de seguridad");
        _txtNewPassword = UiBuilder.AddTextBox(this, 30, 240, 410, password: true, placeholder: "Contraseña nueva");
        _txtConfirm = UiBuilder.AddTextBox(this, 30, 285, 410, password: true, placeholder: "Confirmar contraseña nueva");
        _btnReset = UiBuilder.AddButton(this, "Restablecer", 205, 340, 115);
        _btnReset.Click += (s, e) => _presenter.ResetPassword();
        SetStepTwoVisible(false);

        var btnClose = UiBuilder.AddButton(this, "Cerrar", 325, 340, 115, danger: true);
        btnClose.Click += (s, e) => Close();
    }

    private void SetStepTwoVisible(bool visible)
    {
        _lblQuestion.Visible = visible;
        _txtAnswer.Visible = visible;
        _txtNewPassword.Visible = visible;
        _txtConfirm.Visible = visible;
        _btnReset.Visible = visible;
    }

    public string UserName => _txtUserName.Text.Trim();
    public string Answer => _txtAnswer.Text;
    public string NewPassword => _txtNewPassword.Text;
    public string ConfirmPassword => _txtConfirm.Text;

    public void ShowQuestion(string question)
    {
        _lblQuestion.Text = question;
        _txtUserName.ReadOnly = true;     // el usuario ya no se puede cambiar
        _btnFind.Enabled = false;
        SetStepTwoVisible(true);
        AcceptButton = _btnReset;
        _txtAnswer.Focus();
    }

    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);
    public void CloseView() => Close();
}