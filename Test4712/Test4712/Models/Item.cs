using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Test4712.Models
{
    public class Item: ITableEntity
    {
        [Key]
        public int Item_Id { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        public ETag ETag { get; set; } = ETag.All;
        public DateTimeOffset? Timestamp { get; set; }
    }
}
