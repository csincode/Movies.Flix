using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(s =>
{
    string connectionString = Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING");
    return new CosmosClient(connectionString);
});

builder.Build().Run();
