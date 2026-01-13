using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace Test4712.Controllers
{
    public class ImagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=storagetest2137;AccountKey=wUZ/NjAEcQHxCuKcfvpamZCXggjKTBlb08ON4o/RSBW+OWjE1rVbJ6E+QgjGmenFJOpPrzXaFvr9+AStvXU+hQ==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "itemimages";


        [HttpPost]
        public IActionResult DeleteBlob(string blobName)
        {


            var blobServiceClient = new BlobServiceClient(_connectionString);


            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);


            var blobClient = containerClient.GetBlobClient(blobName);


            var response = blobClient.DeleteIfExists();

            if (response)
            {
                TempData["Message"] = $"Image '{blobName}' deleted successfully.";
            }
            else
            {
                TempData["Message"] = $"Image '{blobName}' could not be found or deleted.";
            }

            return RedirectToAction("ManageImages");
        }

        public IActionResult ManageImages()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            var imageNames = new List<string>();

            foreach (var blobItem in containerClient.GetBlobs())
            {
                imageNames.Add(blobItem.Name);
            }

            return View(imageNames);
        }
    }
}
