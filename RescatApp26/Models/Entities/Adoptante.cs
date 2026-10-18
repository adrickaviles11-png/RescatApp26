namespace RescatApp26.Models.Entities;

public class Adoptante
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public string? PaisOrigen { get; set; }

    public ICollection<Adopcion> Adopciones { get; set; } = new List<Adopcion>();
}