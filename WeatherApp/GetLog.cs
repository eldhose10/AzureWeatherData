using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FetchWeatherData
{
    public class GetLog
    {
        private readonly string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [FunctionName("GetLogs")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getlogs")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function to get logs from azure table.");

            string startDateStr = req.Query["start"];
            string endDateStr = req.Query["end"];

            if (!DateTime.TryParse(startDateStr, out DateTime startDate) || !DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                return new BadRequestObjectResult("Invalid date range");
            }

            var tableClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudTableClient();
            var table = tableClient.GetTableReference("WeatherLogs");

            var query = new TableQuery<WeatherLogEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, startDate),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, endDate)
                )
            );

            var logs = await table.ExecuteQuerySegmentedAsync(query, null);

            return new OkObjectResult(logs.Results); 
        }
    }
}
