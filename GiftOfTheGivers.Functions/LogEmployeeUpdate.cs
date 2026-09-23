using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GiftOfTheGivers.Functions
{
    public class LogEmployeeUpdate
    {
        private readonly ILogger<LogEmployeeUpdate> _logger;

        public LogEmployeeUpdate(ILogger<LogEmployeeUpdate> logger)
        {
            _logger = logger;
        }

        [Function("LogEmployeeUpdate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerClient = new BlobContainerClient(connectionString, "employee-updates");
            await containerClient.CreateIfNotExistsAsync();

            var blobName = $"update-{DateTime.UtcNow:yyyyMMddHHmmssfff}.txt";
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(body)), overwrite: true);

            _logger.LogInformation($"Update logged to blob: {blobName}");

            return new OkObjectResult($"Update logged to blob: {blobName}");
        }
    }
}
