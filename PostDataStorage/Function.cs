using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PostDataStorage;

public class Function
{
    private readonly ILogger<Function> _logger;

    public Function(ILogger<Function> logger)
    {
        _logger = logger;
    }

    [Function("file")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Processando a imagem no storage...");

        try
        {
            if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
            {
                new BadRequestObjectResult("File Type header is missing.");
            }

            var fileType = fileTypeHeader.ToString();
            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("No file uploaded.");
            }

            string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string container = fileType;

            BlobClient blobClient = new BlobClient(connectionString, container, file.FileName);
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, container);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

            string blobName = file.FileName;
            var blob = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            _logger.LogInformation("Imagem processada com sucesso no storage.");

            return new OkObjectResult(new
            {
                message = "File uploaded successfully",
                fileUrl = blob.Uri.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar a imagem.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}