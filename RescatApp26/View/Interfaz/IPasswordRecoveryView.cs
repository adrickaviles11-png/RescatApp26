namespace RescatApp26.View.Interfaz;

public interface IPasswordRecoveryView
{
    string UserName { get; }
    string Answer { get; }
    string NewPassword { get; }
    string ConfirmPassword { get; }
    void ShowQuestion(string question);   // pasa al segundo paso
    void ShowMessage(string message);
    void ShowError(string message);
    void CloseView();
}