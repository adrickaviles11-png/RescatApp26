using Microsoft.Data.SqlClient;
using RescatApp26.Data;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services;

public class ConnectivityService : IConnectivityService
{
    private volatile bool _simulateOffline;

    public bool SimulateOffline
    {
        get => _simulateOffline;
        set => _simulateOffline = value;
    }

    public bool IsOnline()
    {
        if (_simulateOffline) return false;

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
}