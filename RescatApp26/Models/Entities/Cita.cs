namespace RescatApp26.Models.Entities;

public class Cita
{
    public int Id { get; set; }
    public int Tipo { get; set; }
    public int MascotaId { get; set; }
    public int? VeterinariaId { get; set; }
    public int? AdoptanteId { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Motivo { get; set; }
    public int Estado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Mascota Mascota { get; set; } = null!;
    public Veterinaria? Veterinaria { get; set; }
    public Adoptante? Adoptante { get; set; }
}