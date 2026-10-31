namespace RescatApp26.View.Interfaz;

/// <summary>Pantallas que pueden recargar sus datos cuando cambia la cola de sincronización.</summary>
public interface IRefreshable
{
    void RefreshData();
}