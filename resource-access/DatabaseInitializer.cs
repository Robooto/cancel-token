
using Dapper;
using Microsoft.Data.SqlClient;

public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        // Use the master database to check for and create the target database.
        var connectionStringBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"))
        {
            InitialCatalog = "master"
        };

        using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'YourDatabase')
BEGIN
    CREATE DATABASE [YourDatabase];
END";

        _logger.LogInformation("Ensuring that 'YourDatabase' exists...");
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
        _logger.LogInformation("'YourDatabase' is ready.");
    }
}
