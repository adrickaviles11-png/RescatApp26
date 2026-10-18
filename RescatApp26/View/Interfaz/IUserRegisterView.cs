namespace RescatApp26.View.Interfaz;

public interface IUserRegisterView
{
    string UserName { get; }
    string Password { get; }
    string ConfirmPassword { get; }
    void ShowMessage(string message);
    void ShowError(string message);
    void ClearForm();
    void CloseView();
}