using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FunctionApp2.Models
{
    public class Order : ITableEntity
    {
        public string CustomerId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string CustomerName { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }
        public double TotalPrice { get; set; }
        public string FileUrl { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        [JsonIgnore]
        public ETag ETag { get; set; } = ETag.All;

        // String representation for JSON serialization/deserialization
       

    }

}
