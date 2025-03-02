using Dapper;
using Grpc.Core;
using Microsoft.Data.SqlClient;
using greeter_test;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly IConfiguration _configuration;

    public GreeterService(ILogger<GreeterService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        // Check for cancellation or pass the token to a long-running task.
        try
        {
            _logger.LogInformation("Processing gRPC request with cancellation capability.");
            
            // Simulate work that supports cancellation.
            await Task.Delay(2000, context.CancellationToken);

            return new HelloReply
            {
                Message = "Hello " + request.Name
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled by the client.");
            throw new RpcException(new Status(StatusCode.Cancelled, "Operation was canceled by the client."));
        }
    }
    
        // This RPC uses the cancellation token when executing the SQL query.
    public override async Task<SqlReply> SimulateSqlOperationWithCancellation(SqlRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Starting SQL operation with cancellation support.");
        try
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(context.CancellationToken);
            // CommandDefinition allows passing the cancellation token to Dapper.
            var command = new CommandDefinition("WAITFOR DELAY '00:00:05'", cancellationToken: context.CancellationToken);
            await connection.ExecuteAsync(command);
            _logger.LogInformation("SQL operation completed successfully with cancellation support.");
            return new SqlReply { Message = "SQL operation completed with cancellation support." };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SQL operation was canceled by the client.");
            // Here you know cancellation was requested
            return new SqlReply { Message = "SQL operation was canceled gracefully." };
        }
        catch (SqlException ex) when (ex.Message.Contains("cancel", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SQL operation was canceled by the client (SqlException).");
            return new SqlReply { Message = "SQL operation was canceled gracefully." };
        }
    }

    // This RPC does not pass the cancellation token, so the operation will complete even if the client cancels.
    public override async Task<SqlReply> SimulateSqlOperationWithoutCancellation(SqlRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Starting SQL operation without cancellation support.");
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        // The cancellation token is not passed here.
        await connection.ExecuteAsync("WAITFOR DELAY '00:00:05'");
        _logger.LogInformation("SQL operation completed successfully without cancellation support.");
        return new SqlReply { Message = "SQL operation completed without cancellation support." };
    }
}