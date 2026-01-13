using Azure.Data.Tables;
using Azure;
using Test4712.Models;

namespace Test4712.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;
        private readonly TableClient _CustomerProfileTableClient;
        private readonly TableClient _OrderTableClient;
        private readonly TableClient _CheckoutTableClient;


        public TableStorageService(string connectionString)
        {

            _tableClient=new TableClient(connectionString, "Product");
            _CustomerProfileTableClient = new TableClient(connectionString, "CustomerProfile");
            _OrderTableClient = new TableClient(connectionString, "Orders");
            _CheckoutTableClient = new TableClient(connectionString, "CheckoutOrders");
        }

        public async Task AddOrUpdateEntityAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            await _CustomerProfileTableClient.UpsertEntityAsync(entity);

        }

        public async Task<T> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            return await _CustomerProfileTableClient.GetEntityAsync<T>(partitionKey, rowKey);
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            var items = new List<Item>();

            await foreach (var item in _tableClient.QueryAsync<Item>())
            {
                items.Add(item);
            }
            return items;
        }

        public async Task AddItemAsync(Item item)
        {

            if (string.IsNullOrEmpty(item.PartitionKey) || string.IsNullOrEmpty(item.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _tableClient.AddEntityAsync(item);
            }
            catch (RequestFailedException ex)
            {

                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteItemAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);


        }

        public async Task<Item?> GetItemAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Item>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status==484)
            {

                return null;
            }
        }

        public async Task AddCustomerProfileAsync(Users profile)
        {
            if (string.IsNullOrEmpty(profile.PartitionKey) || string.IsNullOrEmpty(profile.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _CustomerProfileTableClient.AddEntityAsync(profile);

            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }



        public async Task DeleteCustomerProfileAsync(string partitionKey, string rowKey)
        {
            await _CustomerProfileTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<Users?> GetCustomerProfileAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _CustomerProfileTableClient.GetEntityAsync<Users>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<Users>> GetAllCustomerProfileAsync()
        {
            var profiles = new List<Users>();

            await foreach (var profile in _CustomerProfileTableClient.QueryAsync<Users>())
            {
                profiles.Add(profile);
            }

            return profiles;
        }


        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            await foreach (var order in _OrderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }

        public async Task<List<Order>> GetOrdersAsync(string customerId)
        {
            var orders = new List<Order>();
            string filter = TableClient.CreateQueryFilter<Order>(o => o.PartitionKey == customerId);

            await foreach (Order order in _OrderTableClient.QueryAsync<Order>(filter: filter))
            {
                orders.Add(order);
            }

            return orders;
        }

        public async Task AddOrderAsync(Order order)
        {

            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");

            }

            try
            {

                await _OrderTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {

                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task<Item> GetItemByIdAsync(int itemId)
        {
            var items = await GetAllItemsAsync();
            return items.FirstOrDefault(i => i.Item_Id == itemId);
        }

        public async Task<Users?> GetCustomerProfileByEmailAsync(string email)
        {
            try
            {
                return await _CustomerProfileTableClient.GetEntityAsync<Users>("CustomerProfiles", email);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            var orders = new List<Order>();
            string filter = TableClient.CreateQueryFilter<Order>(o => o.CustomerId == customerId);

            await foreach (Order order in _OrderTableClient.QueryAsync<Order>(filter: filter))
            {
                orders.Add(order);
            }

            return orders;
        }

        public async Task<List<CheckoutOrder>> GetCheckoutOrdersByCustomerIdAsync(string customerId)
        {
            var orders = new List<CheckoutOrder>();
            string filter = TableClient.CreateQueryFilter<CheckoutOrder>(o => o.CustomerId == customerId);

            await foreach (CheckoutOrder order in _CheckoutTableClient.QueryAsync<CheckoutOrder>(filter: filter))
            {
                orders.Add(order);
            }

            return orders;
        }


        public async Task DeleteOrderAsync(string orderId)
        {

            var tableClient = _OrderTableClient;


            var partitionKey = "OrderPartition";
            var rowKey = orderId;

            try
            {

                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {

                Console.WriteLine($"Error deleting order: {ex.Message}");
                throw;
            }
        }


        public async Task<Order?> GetOrderByIdAsync(string partitionKey, string rowKey)
        {
            try
            {

                var response = await _OrderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {

                return null;
            }
        }

        public async Task AddCheckoutOrderAsync(CheckoutOrder order)
        {

            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _CheckoutTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {

                throw new InvalidOperationException("Error adding entity to Checkout Table", ex);
            }
        }
    }
}
