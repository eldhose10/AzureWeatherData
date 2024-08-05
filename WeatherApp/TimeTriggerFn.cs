using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FetchWeatherData
{
    public class TimeTriggerFn
    {
        private readonly string weatherApiKey = Environment.GetEnvironmentVariable("WeatherApiKey");
        private readonly string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [FunctionName("TimeTriggerFn")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            //log.LogInformation($"TimeTriggerFn Timer trigger function executed at: {DateTime.Now}");

            var tableClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudTableClient();
            var table = tableClient.GetTableReference("WeatherLogs");
            await table.CreateIfNotExistsAsync();

            var blobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("weatherdata");
            await container.CreateIfNotExistsAsync();

            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q=London&appid={weatherApiKey}");
            string payload = await response.Content.ReadAsStringAsync();

            var logEntry = new WeatherLogEntity("WeatherLog", Guid.NewGuid().ToString())
            {
                Timestamp = DateTime.UtcNow,
                Success = response.IsSuccessStatusCode
            };

            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"StatusCode :{logEntry.Success}");
                log.LogInformation($"Timestamp :{logEntry.Timestamp}");

                var blobName = $"{logEntry.PartitionKey}_{logEntry.RowKey}.json";
                var blob = container.GetBlockBlobReference(blobName);
                await blob.UploadTextAsync(payload);
                logEntry.PayloadUri = blob.Uri.ToString();
            }
            else
            {
                log.LogError($"Failed to fetch weather data: {response.StatusCode} - {payload}");
            }

            var insertOperation = TableOperation.Insert(logEntry);
            await table.ExecuteAsync(insertOperation);
        }
    }
}
