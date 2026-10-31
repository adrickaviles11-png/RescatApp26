using RescatApp26.Models.Enums;

namespace RescatApp26.View.Interfaz;

public interface IUserRegisterView
{
    string UserName { get; }
    string Password { get; }
    string ConfirmPassword { get; }
    int RoleCode { get; }
    void SetAvailableRoles(IEnumerable<OptionItem> roles);
    void ShowMessage(string message);
    void ShowError(string message);
    void ClearForm();
    void CloseView();
}