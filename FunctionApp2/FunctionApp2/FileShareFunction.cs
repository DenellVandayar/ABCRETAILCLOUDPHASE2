using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Files.Shares;
using Azure;
using System.Linq;

namespace FunctionApp2
{
    public static class FileShareFunction
    {
        private static string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static string shareName = "productfiles"; // Replace with your share name

        [FunctionName("UploadFile")]
        public static async Task<IActionResult> UploadFile(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploadfile")] HttpRequest req, ILogger log)
        {
            try
            {
                var formCollection = await req.ReadFormAsync();
                var file = formCollection.Files.FirstOrDefault(); // Ensure we get the first file

                if (file == null || file.Length == 0)
                {
                    log.LogError("No file uploaded or file is empty.");
                    return new BadRequestObjectResult("No file uploaded or file is empty.");
                }

                log.LogInformation($"Uploading file: {file.FileName}");

                using var stream = file.OpenReadStream();

                // Initialize ShareClient
                var shareClient = new ShareClient(connectionString, shareName);

                // Check if the File Share exists
                if (!await shareClient.ExistsAsync())
                {
                    log.LogError("Azure File Share does not exist.");
                    return new BadRequestObjectResult("Azure File Share does not exist.");
                }

                // Directly upload to the root directory of the File Share
                var fileClient = shareClient.GetRootDirectoryClient().GetFileClient(file.FileName);

                log.LogInformation("Creating the file in Azure File Share root.");
                await fileClient.CreateAsync(file.Length);
                log.LogInformation("Uploading the file to Azure File Share root.");
                await fileClient.UploadRangeAsync(new HttpRange(0, file.Length), stream);

                log.LogInformation("File uploaded successfully.");
                return new OkObjectResult(fileClient.Uri.ToString());
            }
            catch (RequestFailedException ex)
            {
                log.LogError($"Azure request failed: {ex.Message} - {ex.Status}");
                return new StatusCodeResult(500); // Internal Server Error, with specific Azure exception
            }
            catch (Exception ex)
            {
                log.LogError($"Error during file upload: {ex.Message}");
                return new StatusCodeResult(500); // Internal Server Error for general exceptions
            }
        }


        // Download File
        [FunctionName("DownloadFile")]
        public static async Task<IActionResult> DownloadFile(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "downloadfile/{fileName}")] HttpRequest req,
            string fileName)
        {
            var fileClient = new ShareFileClient(connectionString, shareName, fileName);
            try
            {
                var response = await fileClient.DownloadAsync();
                return new FileStreamResult(response.Value.Content, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (RequestFailedException)
            {
                return new NotFoundResult();
            }
        }

        // Delete File
        [FunctionName("DeleteFile")]
        public static async Task<IActionResult> DeleteFile(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "deletefile/{fileName}")] HttpRequest req,
            string fileName)
        {
            var fileClient = new ShareFileClient(connectionString, shareName, fileName);
            await fileClient.DeleteIfExistsAsync();
            return new OkResult();
        }
    }
}
