using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HealthServer;

internal sealed class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper()
    {
        using var configuration = new ConfigurationManager();
        configuration.SetBasePath(AppContext.BaseDirectory);
        configuration.AddJsonFile("appsettings.json", optional: false);

        var connectionString = configuration.GetConnectionString("HealthInfoDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Thiếu ConnectionStrings:HealthInfoDb trong appsettings.json.");
        }

        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}