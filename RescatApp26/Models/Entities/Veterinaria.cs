namespace RescatApp26.Models.Entities;

public class Veterinaria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool Activa { get; set; } = true;
}
