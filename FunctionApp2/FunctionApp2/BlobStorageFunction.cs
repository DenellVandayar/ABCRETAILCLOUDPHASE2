using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.Collections.Generic;

namespace FunctionApp2
{
    public static class BlobStorageFunction
    {
        private static string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static string containerName = "itemimages"; // Adjust container name if needed

        // Upload Blob
        [FunctionName("UploadBlob")]
        public static async Task<IActionResult> UploadBlob(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")] HttpRequest req)
        {
            var formCollection = await req.ReadFormAsync();
            var file = formCollection.Files[0]; // Assuming a single file upload

            if (file.Length > 0)
            {
                var stream = file.OpenReadStream();
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(file.FileName);
                await blobClient.UploadAsync(stream);
                return new OkObjectResult(blobClient.Uri.ToString());
            }

            return new BadRequestObjectResult("No file uploaded.");
        }

        // List Blobs
        [FunctionName("ListBlobs")]
        public static async Task<IActionResult> ListBlobs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "blobs")] HttpRequest req)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            List<string> blobs = new List<string>();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }

            return new OkObjectResult(blobs);
        }

        // Delete Blob
        [FunctionName("DeleteBlob")]
        public static async Task<IActionResult> DeleteBlob(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "blob/{blobUri}")] HttpRequest req,
            string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            return new OkResult();
        }
    }
}
