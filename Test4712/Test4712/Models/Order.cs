using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace Test4712.Models
{
    public class Order:ITableEntity
    {
        public string CustomerId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string CustomerName { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }
        public double TotalPrice { get; set; }
        public string FileUrl { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        [JsonIgnore]
        public ETag ETag { get; set; }= ETag.All;



    }
}
