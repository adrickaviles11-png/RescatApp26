using RescatApp26.Models.Enums;

namespace RescatApp26.Models.DTOs;

public class PetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public int AgeMonths { get; set; }
    public int Sex { get; set; }
    public int Size { get; set; }
    public string? Description { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public int Status { get; set; }

    // Textos para mostrar en la tabla (solo lectura, se calculan solos).
    public string SexText => PetOptions.SexText(Sex);
    public string SizeText => PetOptions.SizeText(Size);
    public string StatusText => PetOptions.StatusText(Status);
}