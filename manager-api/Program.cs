using Grpc.Core;
using Grpc.Net.Client;
using greeter_test;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint using CancellationToken
app.MapGet("/process", async (CancellationToken ct, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("Starting task delay with CancellationToken.");
            await Task.Delay(10000, ct);
            return Results.Ok("Completed using CancellationToken");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Task delay was canceled.");
            return Results.Problem("Operation was canceled.");
        }
    })
    .WithName("ProcessWithCancellation")
    .WithOpenApi();

// Endpoint without CancellationToken support
app.MapGet("/processWithout", async (ILogger<Program> logger) =>
    {
        logger.LogInformation("Starting task delay without CancellationToken. ${DateTime.Now}", DateTime.Now);
        await Task.Delay(5000);
        logger.LogInformation("Task delay completed successfully. ${DateTime.Now}", DateTime.Now);
        return Results.Ok("Completed without CancellationToken");
    })
    .WithName("ProcessWithoutCancellation")
    .WithOpenApi();

app.MapGet("/grpc-hello-with-token", async (CancellationToken ct, ILogger<Program> logger) =>
    {
        try
        {
            using var channel = GrpcChannel.ForAddress("http://resource-access:5001");
            var client = new Greeter.GreeterClient(channel);
            var response = await client.SayHelloAsync(new HelloRequest { Name = "World" }, cancellationToken: ct);
            return Results.Ok(response.Message);
        }
        catch (RpcException)
        {
            logger.LogWarning("Task delay was canceled.");
            return Results.Problem("Operation was canceled.");
        }
    })
    .WithName("GrpcHelloWithToken")
    .WithOpenApi();

app.MapGet("/grpc-hello-no-token", async (ILogger<Program> logger) =>
    {
        using var channel = GrpcChannel.ForAddress("http://resource-access:5001");
        var client = new Greeter.GreeterClient(channel);
        var response = await client.SayHelloAsync(new HelloRequest { Name = "World" });
        logger.LogInformation("Task delay completed successfully.");
        return Results.Ok(response.Message);
    })
    .WithName("GrpcHelloNoToken")
    .WithOpenApi();

app.MapGet("/grpc-sql-op-with-token", async (CancellationToken ct, ILogger<Program> logger) =>
    {
        try
        {
            using var channel = GrpcChannel.ForAddress("http://resource-access:5001");
            var client = new Greeter.GreeterClient(channel);
            var response = await client.SimulateSqlOperationWithCancellationAsync(new SqlRequest { Query = "Execute SQL" }, cancellationToken: ct);
            return Results.Ok(response.Message);
        }
        catch (RpcException)
        {
            logger.LogWarning("SQL operation was canceled. ${DateTime.Now}", DateTime.Now);
            return Results.Problem("SQL operation was canceled.");
        }
    })
    .WithName("GrpcSqlOperationWithToken")
    .WithOpenApi();

app.MapGet("/grpc-sql-op-no-token", async (ILogger<Program> logger) =>
    {
        using var channel = GrpcChannel.ForAddress("http://resource-access:5001");
        var client = new Greeter.GreeterClient(channel);
        var response = await client.SimulateSqlOperationWithoutCancellationAsync(new SqlRequest { Query = "Execute SQL" });
        logger.LogInformation("SQL operation completed successfully without cancellation support. ${DateTime.Now}", DateTime.Now);
        return Results.Ok(response.Message);
    })
    .WithName("GrpcSqlOperationWithoutToken")
    .WithOpenApi();

app.Run();