using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;
using FunctionApp2.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace FunctionApp2
{
    public static class Function1


    {

 

        private static string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // Add or Update Entity
        [FunctionName("AddOrUpdateEntity")]
        public static async Task<IActionResult> AddOrUpdateEntity(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "entity")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var entity = JsonConvert.DeserializeObject<ITableEntity>(requestBody);

            var tableClient = new TableClient(connectionString, "Items"); // Adjust table name
            await tableClient.UpsertEntityAsync(entity);

            return new OkResult();
        }

        // Get Entity
        [FunctionName("GetEntity")]
        public static async Task<IActionResult> GetEntity(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "entity/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(connectionString, "Items"); // Adjust table name

            try
            {
                var response = await tableClient.GetEntityAsync<ITableEntity>(partitionKey, rowKey);
                return new OkObjectResult(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundResult();
            }
        }

        // Get All Items
        [FunctionName("GetAllItems")]
        public static async Task<IActionResult> GetAllItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items")] HttpRequest req)
        {
            var tableClient = new TableClient(connectionString, "Items"); // Adjust table name
            List<Item> items = new List<Item>();

            await foreach (var item in tableClient.QueryAsync<Item>())
            {
                items.Add(item);
            }

            return new OkObjectResult(items);
        }

        // Add Item
        [FunctionName("AddItem")]
        public static async Task<IActionResult> AddItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "item")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var item = JsonConvert.DeserializeObject<Item>(requestBody);

            if (string.IsNullOrEmpty(item.PartitionKey) || string.IsNullOrEmpty(item.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            var tableClient = new TableClient(connectionString, "Items"); // Adjust table name
            await tableClient.AddEntityAsync(item);
            return new OkResult();
        }

        // Delete Item
        [FunctionName("DeleteItem")]
        public static async Task<IActionResult> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "item/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(connectionString, "Items"); // Adjust table name
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            return new OkResult();
        }

        // Get Item
        [FunctionName("GetItem")]
        public static async Task<IActionResult> GetItem(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "item/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(connectionString, "Items"); // Adjust table name

            try
            {
                var response = await tableClient.GetEntityAsync<Item>(partitionKey, rowKey);
                return new OkObjectResult(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundResult();
            }
        }

        // Add Customer Profile
        [FunctionName("AddCustomerProfile")]
        public static async Task<IActionResult> AddCustomerProfile(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customerProfile")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var profile = JsonConvert.DeserializeObject<Users>(requestBody);

            if (string.IsNullOrEmpty(profile.PartitionKey) || string.IsNullOrEmpty(profile.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            var tableClient = new TableClient(connectionString, "CustomerProfiles"); // Adjust table name
            await tableClient.AddEntityAsync(profile);
            return new OkResult();
        }

        // Delete Customer Profile
        [FunctionName("DeleteCustomerProfile")]
        public static async Task<IActionResult> DeleteCustomerProfile(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customerProfile/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(connectionString, "CustomerProfiles"); // Adjust table name
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            return new OkResult();
        }

        // Get Customer Profile
        [FunctionName("GetCustomerProfile")]
        public static async Task<IActionResult> GetCustomerProfile(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customerProfile/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(connectionString, "CustomerProfiles"); // Adjust table name

            try
            {
                var response = await tableClient.GetEntityAsync<Users>(partitionKey, rowKey);
                return new OkObjectResult(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundResult();
            }
        }

        // Get All Customer Profiles
        [FunctionName("GetAllCustomerProfiles")]
        public static async Task<IActionResult> GetAllCustomerProfiles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customerProfiles")] HttpRequest req)
        {
            var tableClient = new TableClient(connectionString, "CustomerProfiles"); // Adjust table name
            List<Users> profiles = new List<Users>();

            await foreach (var profile in tableClient.QueryAsync<Users>())
            {
                profiles.Add(profile);
            }

            return new OkObjectResult(profiles);
        }

        // Get All Orders
        [FunctionName("GetAllOrders")]
        public static async Task<IActionResult> GetAllOrders(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequest req)
        {
            var tableClient = new TableClient(connectionString, "Orders"); // Adjust table name
            List<Order> orders = new List<Order>();

            await foreach (var order in tableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }

            return new OkObjectResult(orders);
        }

       
        [FunctionName("GetOrdersByCustomerId")]
        public static async Task<IActionResult> GetOrdersByCustomerId(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/customer/{customerId}")] HttpRequest req,
    string customerId)
        {
            var tableClient = new TableClient(connectionString, "Orders"); // Adjust table name

            List<Order> orders = new List<Order>();

            // Instead of filtering by PartitionKey, we filter by CustomerId field
            string filter = TableClient.CreateQueryFilter<Order>(o => o.CustomerId == customerId);

            // Query for orders where CustomerId matches the provided customerId (email)
            await foreach (Order order in tableClient.QueryAsync<Order>(filter: filter))
            {
                orders.Add(order);
            }

            return new OkObjectResult(orders);
        }

        // Add Order
        [FunctionName("AddOrder")]
        public static async Task<IActionResult> AddOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "order")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);

            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            var tableClient = new TableClient(connectionString, "Orders"); // Adjust table name
            await tableClient.AddEntityAsync(order);
            return new OkResult();
        }

        // Get Order by ID
        [FunctionName("GetOrderById")]
        public static async Task<IActionResult> GetOrderById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "order/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(connectionString, "Orders"); // Adjust table name

            try
            {
                var response = await tableClient.GetEntityAsync<Order>(partitionKey, rowKey);
                return new OkObjectResult(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundResult();
            }
        }

        // Delete Order
        [FunctionName("DeleteOrder")]
        public static async Task<IActionResult> DeleteOrder(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "order/{orderId}")] HttpRequest req,
            string orderId)
        {
            var tableClient = new TableClient(connectionString, "Orders"); // Adjust table name
            var partitionKey = "OrderPartition"; // Adjust as necessary

            await tableClient.DeleteEntityAsync(partitionKey, orderId);
            return new OkResult();
        }

        // Add Checkout Order
        [FunctionName("AddCheckoutOrder")]
        public static async Task<IActionResult> AddCheckoutOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "checkoutOrder")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<CheckoutOrder>(requestBody);

            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            var tableClient = new TableClient(connectionString, "CheckoutOrders"); // Adjust table name
            await tableClient.AddEntityAsync(order);
            return new OkResult();
        }

        // Get Checkout Orders by Customer ID
        [FunctionName("GetCheckoutOrdersByCustomerId")]
        public static async Task<IActionResult> GetCheckoutOrdersByCustomerId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "checkoutOrders/customer/{customerId}")] HttpRequest req,
            string customerId)
        {
            var tableClient = new TableClient(connectionString, "CheckoutOrders"); // Adjust table name
            List<CheckoutOrder> orders = new List<CheckoutOrder>();
            string filter = TableClient.CreateQueryFilter<CheckoutOrder>(o => o.CustomerId == customerId);

            await foreach (CheckoutOrder order in tableClient.QueryAsync<CheckoutOrder>(filter: filter))
            {
                orders.Add(order);
            }

            return new OkObjectResult(orders);
        }

        [FunctionName("GetItemById")]
        public static async Task<IActionResult> GetItemById(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "item/{partitionKey}/{rowKey}")] HttpRequest req,
         string partitionKey,
         string rowKey)
        {
            var tableClient = new TableClient(connectionString, "Items"); // Make sure the table name is correct

            try
            {
                // Retrieve the specific item from the Azure Table Storage
                var response = await tableClient.GetEntityAsync<Item>(partitionKey, rowKey);
                return new OkObjectResult(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundResult(); // Return 404 if item not found
            }
            catch (Exception)
            {
                return new StatusCodeResult(500); // Return 500 for any other error
            }

        }

        [FunctionName("GetItemsById")]
        public static async Task<IActionResult> GetItemsById(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "item/{itemId}")] HttpRequest req,
    int itemId)
        {
            var tableClient = new TableClient(connectionString, "Items"); // Ensure the table name is correct
            List<Item> items = new List<Item>();

            // Fetch all items (or you can optimize this with filters if needed)
            await foreach (var item in tableClient.QueryAsync<Item>())
            {
                items.Add(item);
            }

            // Find the item with the specified Item_Id
            var itemById = items.FirstOrDefault(i => i.Item_Id == itemId);

            if (itemById == null)
            {
                return new NotFoundResult(); // Return 404 if the item is not found
            }

            return new OkObjectResult(itemById);
        }

    }
}
