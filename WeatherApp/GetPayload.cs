using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FetchWeatherData
{
    public class GetPayload
    {
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [FunctionName("GetPayload")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getpayload")] HttpRequest req,
            ILogger log)
        {
            string logId = req.Query["logId"];

            if (string.IsNullOrEmpty(logId))
            {
                return new BadRequestObjectResult("Please provide a logId.");
            }

            // Create a CloudBlobClient
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Reference the container where the payloads are stored
            CloudBlobContainer container = blobClient.GetContainerReference("weatherdata"); 

            // Reference the blob using the logId
            CloudBlockBlob blob = container.GetBlockBlobReference($"{logId}.json");

            if (await blob.ExistsAsync())
            {
                // Download the blob content
                var stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;

                // Read the content
                using (var reader = new StreamReader(stream))
                {
                    var payload = await reader.ReadToEndAsync();
                    return new ContentResult
                    {
                        Content = payload,
                        ContentType = "application/json",
                        StatusCode = StatusCodes.Status200OK
                    };
                }
            }
            else
            {
                return new NotFoundObjectResult($"Payload with logId {logId} not found.");
            }
        }
    }
}
