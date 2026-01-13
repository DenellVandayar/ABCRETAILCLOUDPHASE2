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
using FunctionApp2.Models;

namespace FunctionApp2
{
    public static class UserTableFunction
    {
        private static string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static string tableName = "UsersTable"; // Replace with your actual table name
        private static TableClient tableClient = new TableServiceClient(connectionString).GetTableClient(tableName);

        static UserTableFunction()
        {
            tableClient.CreateIfNotExists(); // Ensure the table exists
        }

        // Add User
        [FunctionName("AddUser")]
        public static async Task<IActionResult> AddUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "adduser")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonConvert.DeserializeObject<Users>(requestBody); // Assuming Users is a valid class

            if (user == null)
            {
                return new BadRequestObjectResult("Invalid user data.");
            }

            await tableClient.AddEntityAsync(user);
            return new OkResult();
        }

        // Get User by Email
        [FunctionName("GetUserByEmail")]
        public static async Task<IActionResult> GetUserByEmail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getuser/{email}")] HttpRequest req,
            string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult("Email cannot be null or empty.");
            }

            var query = tableClient.QueryAsync<Users>(u => u.Email == email);
            await foreach (var user in query)
            {
                // Log or debug output for checking user
                Console.WriteLine($"User found: {user.Email}");
                return new OkObjectResult(user);
            }

            return new NotFoundResult(); // Return 404 if user not found
        }
    }
}
