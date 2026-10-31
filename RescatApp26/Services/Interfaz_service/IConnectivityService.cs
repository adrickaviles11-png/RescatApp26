namespace RescatApp26.Services.Interfaz_service;

public interface IConnectivityService
{
    bool SimulateOffline { get; set; }

    /// <summary>Comprueba si SQL Server responde (espera como máximo 2 segundos).</summary>
    bool IsOnline();
}