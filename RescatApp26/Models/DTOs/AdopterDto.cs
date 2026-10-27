namespace RescatApp26.Models.DTOs;

public class AdopterDto
{
    public int Id { get; set; }
    public string FirstNames { get; set; } = string.Empty;
    public string LastNames { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    public string FullName => $"{FirstNames} {LastNames}";
}