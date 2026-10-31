namespace RescatApp26.Services.Interfaz_service;

public interface IConnectivityService
{
    /// <summary>Último estado conocido de SQL Server. Se actualiza solo, cada 3 segundos.</summary>
    bool IsOnline { get; }

    /// <summary>Sondea SQL Server ahora mismo (espera como máximo 2 segundos) y actualiza el estado.</summary>
    bool CheckNow();

    /// <summary>Se dispara cuando cambia el estado. El valor es true si volvió la conexión.</summary>
    event EventHandler<bool>? StatusChanged;

    void Start();
}