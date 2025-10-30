using GetMoviesDetail.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GetMoviesDetail;

public class Function
{
    private readonly ILogger<Function> _logger;
    private readonly CosmosClient _cosmosClient;

    public Function(ILogger<Function> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
    }

    [Function("detail")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var container = _cosmosClient.GetContainer(
            "MovieFlixDB",
            "Movies"
        );

        var id = req.Query["id"].ToString();
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        var resultSet = container.GetItemQueryIterator<MovieResult>(query);
        var results = new List<MovieResult>();

        while (resultSet.HasMoreResults)
        {
            foreach (var item in await resultSet.ReadNextAsync())
            {
                results.Add(item);
            }
        }

        var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await responseMessage.WriteAsJsonAsync(results.FirstOrDefault());

        return responseMessage;
    }
}