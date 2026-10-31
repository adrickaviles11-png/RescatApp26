using Microsoft.Data.SqlClient;
using RescatApp26.Data;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services;

/// <summary>Vigila la conexión con SQL Server en segundo plano y avisa cuando se pierde o vuelve.</summary>
public class ConnectivityService : IConnectivityService, IDisposable
{
    private readonly object _lock = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _isOnline;

    public event EventHandler<bool>? StatusChanged;

    public bool IsOnline
    {
        get { lock (_lock) return _isOnline; }
    }

    public void Start()
    {
        _isOnline = Probe();
        _ = Task.Run(() => MonitorAsync(_cts.Token));
    }

    public bool CheckNow()
    {
        bool online = Probe();
        Update(online);
        return online;
    }

    private async Task MonitorAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
                Update(Probe());
        }
        catch (OperationCanceledException)
        {
            // La aplicación se está cerrando.
        }
    }

    private void Update(bool online)
    {
        bool changed;
        lock (_lock)
        {
            changed = _isOnline != online;
            _isOnline = online;
        }
        if (changed) StatusChanged?.Invoke(this, online);
    }

    private static bool Probe()
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(RescatAppDbContext.GetConnectionString())
            {
                ConnectTimeout = 2
            };
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}