using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PostDatabase.Models;

namespace PostDatabase;

public class Function
{
    private readonly ILogger<Function> _logger;

    public Function(ILogger<Function> logger)
    {
        _logger = logger;
    }

    [Function("movie")]
    [CosmosDBOutput("%DATABASE_NAME%", "%COSMOS_CONTAINER_NAME%", Connection = "COSMOS_DB_CONNECTION_STRING", CreateIfNotExists = true, PartitionKey = "id")]
    public async Task<object?> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        MovieRequest? movieRequest = null;

        var content = await new StreamReader(req.Body).ReadToEndAsync();

        try
        {
            movieRequest = JsonConvert.DeserializeObject<MovieRequest>(content);

        }
        catch (System.Exception ex)
        {
            _logger.LogError("Error deserializing request body: {ErrorMessage}", ex.Message);
            return new BadRequestObjectResult("Invalid request body.");
        }

        return movieRequest;
    }
}