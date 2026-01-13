using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Test4712.Models;
using Test4712.Services;

namespace Test4712.Controllers
{
    public class ItemsController : Controller
    {
        

       
        private readonly HttpClient _httpClient;
        



        public ItemsController( HttpClient httpClient)
        {
            
            _httpClient = httpClient;
            
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/items?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
            if (response.IsSuccessStatusCode)
            {
                var itemJson = await response.Content.ReadAsStringAsync();
                var items = JsonConvert.DeserializeObject<List<Item>>(itemJson);
                return View(items);
            }
            return View(new List<Item>());
        }

        [HttpGet]
        public IActionResult AddItem()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(Item item, IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(stream), "file", file.FileName);

                // Send file to Azure Function for upload
                var uploadResponse = await _httpClient.PostAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/upload?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D", content);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var imageUrl = await uploadResponse.Content.ReadAsStringAsync();
                    item.ImageUrl = imageUrl; // Set the uploaded image URL
                }
                else
                {
                    var errorResponse = await uploadResponse.Content.ReadAsStringAsync();
                    return BadRequest($"Failed to upload file. Server responded with: {errorResponse}");
                }
            }

            
            // Get all items to determine the next Item_Id
            var itemsResponse = await _httpClient.GetAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/items?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");

            if (itemsResponse.IsSuccessStatusCode)
            {
                var itemsJson = await itemsResponse.Content.ReadAsStringAsync();
                var items = JsonConvert.DeserializeObject<List<Item>>(itemsJson);

                // Set the next Item_Id
                item.Item_Id = items.Any() ? items.Max(i => i.Item_Id) + 1 : 1;
            }
            else
            {
                // Log the error response for debugging
                var errorResponse = await itemsResponse.Content.ReadAsStringAsync();
                return BadRequest($"Failed to retrieve items: {errorResponse}");
            }


            // Set PartitionKey and RowKey
            item.PartitionKey = "ItemsPartition";
            item.RowKey = Guid.NewGuid().ToString();

            // Add the item to Table Storage via Azure Function
            var itemJson = JsonConvert.SerializeObject(item);
            var addItemResponse = await _httpClient.PostAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/item?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D", new StringContent(itemJson, Encoding.UTF8, "application/json"));
            if (!addItemResponse.IsSuccessStatusCode)
            {
                return BadRequest("Failed to add item.");
            }

            // Send a message to the queue via Azure Function
            string message = $"New Item Added To Inventory {item.Item_Id} {item.Name} {item.Price} {item.Timestamp}.";
            var queueResponse = await _httpClient.PostAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/sendinventory?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D", new StringContent(message, Encoding.UTF8, "application/json"));
            if (!queueResponse.IsSuccessStatusCode)
            {
                return BadRequest("Failed to send message to queue.");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteItemAsync(string partitionKey, string rowKey)
        {
            using (var client = new HttpClient())
            {
                // Replace with your actual Azure Function URL
                string functionUrl = $"https://cloudfunctionappst10373357.azurewebsites.net/api/item/{partitionKey}/{rowKey}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";

                // Send DELETE request to the Azure Function
                var response = await client.DeleteAsync(functionUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Handle success
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle failure (e.g., log the error, display a message to the user, etc.)
                    ModelState.AddModelError(string.Empty, "Unable to delete item.");
                    return RedirectToAction("Index");
                }
            }
        }

    }
}
