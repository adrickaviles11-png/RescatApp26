namespace RescatApp26.Models.Enums;

// Los números son los que guarda la columna Usuarios.Rol.
public enum UserRole { Administrador = 0, Voluntario = 1, Adoptante = 2 }

public static class UserRoleOptions
{
    public static readonly IReadOnlyList<OptionItem> All = new[]
    {
        new OptionItem((int)UserRole.Administrador, "Administrador"),
        new OptionItem((int)UserRole.Voluntario, "Voluntario"),
        new OptionItem((int)UserRole.Adoptante, "Adoptante")
    };

    public static string Text(int value) => All.FirstOrDefault(o => o.Value == value)?.Text ?? "Desconocido";
}

/// <summary>Qué puede hacer cada rol. Todos los permisos de la aplicación están en este lugar.</summary>
public static class AppPermissions
{
    public static bool CanManageUsers(int role) => role == (int)UserRole.Administrador;
    public static bool CanDelete(int role) => role == (int)UserRole.Administrador;
    public static bool CanEdit(int role) => role == (int)UserRole.Administrador || role == (int)UserRole.Voluntario;
    public static bool CanUseStaffModules(int role) => CanEdit(role);
}