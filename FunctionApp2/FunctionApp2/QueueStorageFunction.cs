using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Queues;

namespace FunctionApp2
{
    public static class QueueStorageFunction
    {
        private static string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // Send message to Item Inventory Queue
        [FunctionName("SendMessageToInventoryQueue")]
        public static async Task<IActionResult> SendMessageToInventoryQueue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sendinventory")] HttpRequest req)
        {
            string message = await new StreamReader(req.Body).ReadToEndAsync();

            var queueClient = new QueueClient(connectionString, "iteminventory");
            await queueClient.CreateIfNotExistsAsync(); // Ensure the queue exists
            await queueClient.SendMessageAsync(message);

            return new OkResult();
        }

        // Send message to Item Order Queue
        [FunctionName("SendOrderMessage")]
        public static async Task<IActionResult> SendOrderMessage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sendorder")] HttpRequest req)
        {
            string message = await new StreamReader(req.Body).ReadToEndAsync();

            var orderQueueClient = new QueueClient(connectionString, "itemorder");
            await orderQueueClient.CreateIfNotExistsAsync(); // Ensure the queue exists
            await orderQueueClient.SendMessageAsync(message);

            return new OkResult();
        }
    }
}
