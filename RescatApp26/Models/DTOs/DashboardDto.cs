namespace RescatApp26.Models.DTOs;

public class ChartItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class DashboardDto
{
    public int AvailablePets { get; set; }
    public int AdoptedPets { get; set; }
    public int Adopters { get; set; }
    public int AdoptionsThisMonth { get; set; }
    public int CitasThisWeek { get; set; }
    public int CitasFirstHalf { get; set; }
    public int CitasThisMonth { get; set; }
    public List<ChartItemDto> CitasByStatus { get; set; } = new();
    public List<ChartItemDto> CitasByType { get; set; } = new();
}