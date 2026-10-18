namespace RescatApp26.Models.Entities;

public class Adopcion
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public int AdoptanteId { get; set; }
    public DateTime FechaAdopcion { get; set; } = DateTime.Now;
    public string? Observaciones { get; set; }
    public decimal CostoAdopcion { get; set; }
    public int CodigoAdopcion { get; set; }

    public Mascota Mascota { get; set; } = null!;
    public Adoptante Adoptante { get; set; } = null!;
}