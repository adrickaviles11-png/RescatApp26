namespace RescatApp26.Models.Entities;

public class Mascota
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public int EdadMeses { get; set; }
    public int Sexo { get; set; }
    public int Tamano { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public int Estado { get; set; }

    public ICollection<Adopcion> Adopciones { get; set; } = new List<Adopcion>();
}