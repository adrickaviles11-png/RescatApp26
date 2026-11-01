namespace RescatApp26.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int Rol { get; set; } = 1;
    public string? PreguntaSeguridad { get; set; }
    public string? RespuestaHash { get; set; }
}