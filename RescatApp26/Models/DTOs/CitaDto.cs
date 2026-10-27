using RescatApp26.Models.Enums;

namespace RescatApp26.Models.DTOs;

public class CitaDto
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int PetId { get; set; }
    public int? VeterinaryId { get; set; }
    public int? AdopterId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Reason { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }

    // Para mostrar en la tabla
    public string PetName { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public string TypeText => CitaOptions.TypeText(Type);
    public string StatusText => CitaOptions.StatusText(Status);
}