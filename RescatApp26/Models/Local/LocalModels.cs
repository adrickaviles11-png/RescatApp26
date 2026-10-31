namespace RescatApp26.Models.Local;

public static class QueueStates
{
    public const string Pendiente = "Pendiente";
    public const string Enviado = "Enviado";
    public const string Error = "Error";
}

/// <summary>Un elemento de la cola FIFO que vive en SQLite.</summary>
public class ColaItem
{
    public long Id { get; set; }                       // AUTOINCREMENT: define el orden de llegada
    public string Entidad { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;   // los datos, en JSON
    public string Estado { get; set; } = QueueStates.Pendiente;
    public int Intentos { get; set; }
    public string? UltimoError { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaEnvio { get; set; }
}

/// <summary>Copia local de la última lista descargada, para mostrar datos sin conexión.</summary>
public class CacheItem
{
    public string Clave { get; set; } = string.Empty;
    public string Json { get; set; } = string.Empty;
    public DateTime Actualizado { get; set; } = DateTime.Now;
}