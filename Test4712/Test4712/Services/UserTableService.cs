using Azure.Data.Tables;
using Test4712.Models;

namespace Test4712.Services
{
    public class UserTableService
    {
        private readonly TableClient _tableClient;

        public UserTableService(string connectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task AddUserAsync(Users user)
        {
            await _tableClient.AddEntityAsync(user);
        }

       
        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            if (_tableClient == null)
            {
                throw new InvalidOperationException("Table client is not initialized.");
            }

            try
            {
                var query = _tableClient.QueryAsync<Users>(u => u.Email == email);

                await foreach (var user in query)
                {
                    // Log or debug output for checking user
                    Console.WriteLine($"User found: {user.Email}");

                    return user; // Return the first user found
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Error querying user: {ex.Message}");
                throw;
            }

            return null; // Return null if no user is found
        }



    }
}
