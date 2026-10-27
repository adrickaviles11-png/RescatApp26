namespace RescatApp26.Models.DTOs;

/// <summary>Un par id/texto para llenar listas desplegables.</summary>
public class LookupItemDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class AdoptionCreateDto
{
    public int PetId { get; set; }
    public int AdopterId { get; set; }
    public decimal Cost { get; set; }
    public string? Notes { get; set; }
}

public class AdoptionListDto
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string AdopterName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
    public string? Notes { get; set; }
}