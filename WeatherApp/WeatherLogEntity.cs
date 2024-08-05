using Microsoft.WindowsAzure.Storage.Table;
using System; 
public class WeatherLogEntity : TableEntity
{
    public WeatherLogEntity() { }
    public WeatherLogEntity(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string PayloadUri { get; set; }
}