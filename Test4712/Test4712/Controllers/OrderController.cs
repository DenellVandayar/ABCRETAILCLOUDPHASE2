using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Test4712.Models;
using Test4712.Services;

namespace Test4712.Controllers
{
    public class OrderController : Controller
    {
        

       
        private readonly HttpClient _httpClient;


        public OrderController(HttpClient httpClient)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(Order order, IFormFile file)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            // Step 1: Get User by Email using Azure Function
            var userFunctionUrl = $"https://cloudfunctionappst10373357.azurewebsites.net/api/getuser/{email}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";
            var userResponse = await _httpClient.GetAsync(userFunctionUrl);
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found.");
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<Users>(userJson);

            // Step 2: Get All Items using Azure Function
            var getAllItemsUrl = "https://cloudfunctionappst10373357.azurewebsites.net/api/items?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";
            var allItemsResponse = await _httpClient.GetAsync(getAllItemsUrl);
            if (!allItemsResponse.IsSuccessStatusCode)
            {
                return BadRequest("Failed to retrieve items.");
            }

            var allItemsJson = await allItemsResponse.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<Item>>(allItemsJson);

            // Step 3: Filter to Get the Item by ItemId
            var selectedItem = items.FirstOrDefault(i => i.Item_Id == order.ItemId);
            if (selectedItem == null)
            {
                return NotFound("Item not found.");
            }

            // Step 4: Prepare Order Details
            order.TotalPrice = selectedItem.Price;
            order.CustomerName = users.FullName;
            order.ItemId = selectedItem.Item_Id;
            order.CustomerId = users.Email;
            order.ItemName = selectedItem.Name;
            order.PartitionKey = "OrderPartition";
            order.RowKey = Guid.NewGuid().ToString();

            // Step 5: Upload File via Azure Function
            string fileUrl = null;
            if (file != null && file.Length > 0)
            {
                try
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var formData = new MultipartFormDataContent();

                        // Prepare the file stream content
                        var streamContent = new StreamContent(stream);
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                        // Add file content to form data
                        formData.Add(streamContent, "file", file.FileName);

                        // Upload file via Azure Function
                        var uploadFunctionUrl = "https://cloudfunctionappst10373357.azurewebsites.net/api/uploadfile?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";

                        // Send the HTTP request
                        var uploadResponse = await _httpClient.PostAsync(uploadFunctionUrl, formData);

                        // Check if the upload was successful
                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            fileUrl = await uploadResponse.Content.ReadAsStringAsync();
                            order.FileUrl = fileUrl;
                        }
                        else
                        {
                            // Log or return a more detailed error message
                            var errorResponse = await uploadResponse.Content.ReadAsStringAsync();
                            return BadRequest($"File upload failed: {uploadResponse.StatusCode} - {errorResponse}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Catch and log any exceptions that occur during the file upload
                    return BadRequest($"File upload error: {ex.Message}");
                }
            }
            else
            {
                return BadRequest("No file was uploaded or the file is empty.");
            }


            // Step 6: Call Azure Function to Add the Order
            var orderJson = JsonConvert.SerializeObject(order);
            var orderContent = new StringContent(orderJson, Encoding.UTF8, "application/json");

            var addOrderFunctionUrl = "https://cloudfunctionappst10373357.azurewebsites.net/api/order?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";
            var addOrderResponse = await _httpClient.PostAsync(addOrderFunctionUrl, orderContent);
            if (addOrderResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("OrderConfirm");
            }

            return BadRequest("Failed to create order.");
        }



        public IActionResult OrderConfirm()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Order(int itemId)
        {
            var model = new Order
            {
                ItemId = itemId

            };
            return View(model);
        }



        public async Task<IActionResult> Orders()
        {
            // Assuming customer ID is stored as the user's email
            var customerId = User.FindFirstValue(ClaimTypes.Email);
            double totalPrice = 0;

            // Call the GetOrdersByCustomerId Azure Function
            var ordersResponse = await _httpClient.GetFromJsonAsync<List<Order>>($"https://cloudfunctionappst10373357.azurewebsites.net/api/orders/customer/{customerId}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");

            // Check if orders were found
            if (ordersResponse == null || !ordersResponse.Any())
            {
                ordersResponse = new List<Order>();
            }

            // Store the orders received from the Azure Function
            var orders = ordersResponse;

            // Iterate through each order and fetch item details from GetItemById Azure Function
            foreach (var cartItem in orders)
            {
                // Assuming cartItem.ItemId is a property of Order that corresponds to the item
                var itemResponse = await _httpClient.GetFromJsonAsync<Item>($"https://cloudfunctionappst10373357.azurewebsites.net/api/item/{cartItem.ItemId}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");

                // Check if item response is valid before using it
                if (itemResponse != null)
                {
                    totalPrice += itemResponse.Price;
                }
            }

            // Pass the total price to the view if needed
            ViewBag.TotalPrice = totalPrice;

            // Return the orders to the view
            return View(orders);
        }







        public async Task<IActionResult> DownloadFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return NotFound();
            }

            // Extract file name from file URL
            var fileName = fileUrl.Split('/').Last();
            fileName = Uri.UnescapeDataString(fileName);

            // URL of the Azure Function
            string azureFunctionUrl = $"https://cloudfunctionappst10373357.azurewebsites.net/api/downloadfile/{fileName}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";

            // Make HTTP GET request to the Azure Function
            var response = await _httpClient.GetAsync(azureFunctionUrl);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            // Read the content as a stream
            var fileStream = await response.Content.ReadAsStreamAsync();

            return File(fileStream, "application/octet-stream", fileName);
        }


        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest("Invalid order ID.");
            }

            var partitionKey = "OrderPartition";

            // Call the Azure Function to get the order by ID
            var orderResponse = await _httpClient.GetAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/order/{partitionKey}/{orderId}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
            if (!orderResponse.IsSuccessStatusCode)
            {
                return NotFound("Order not found.");
            }

            var orderContent = await orderResponse.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<Order>(orderContent);

            try
            {
                // Call the Azure Function to delete the order
                await _httpClient.DeleteAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/order/{orderId}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");

                // If there is a file URL, call the Azure Function to delete the file
                if (!string.IsNullOrEmpty(order.FileUrl))
                {
                    var fileName = order.FileUrl.Split('/').Last();
                    await _httpClient.DeleteAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/deletefile/{fileName}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
                }

                return RedirectToAction("Orders");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                // Call the Azure Function to get all orders
                var ordersResponse = await _httpClient.GetAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/orders?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
                if (!ordersResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Orders");
                }

                var ordersContent = await ordersResponse.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(ordersContent);

                if (orders.Count == 0)
                {
                    return RedirectToAction("Orders");
                }

                // Iterate through orders and process each
                foreach (var order in orders)
                {
                    var checkoutOrder = new CheckoutOrder
                    {
                        PartitionKey = order.PartitionKey,
                        RowKey = order.RowKey,
                        ItemName = order.ItemName,
                        CustomerName = order.CustomerName,
                        Address = order.Address,
                        PhoneNumber = order.PhoneNumber,
                        ItemId = order.ItemId,
                        CustomerId = order.CustomerId,
                        TotalPrice = order.TotalPrice,
                        FileUrl = order.FileUrl,
                    };

                    // Call the Azure Function to add checkout order
                    var addCheckoutOrderContent = new StringContent(JsonConvert.SerializeObject(checkoutOrder), Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/checkoutOrder?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D", addCheckoutOrderContent);

                    // Prepare and send the order message via Azure Function
                    string message = $"Order processed successfully {checkoutOrder.ItemId} {checkoutOrder.ItemName} {checkoutOrder.TotalPrice} {checkoutOrder.Timestamp}.";
                    var sendMessageContent = new StringContent(message, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/sendorder?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D", sendMessageContent);
                }

                // Iterate through orders and delete each via Azure Function
                foreach (var order in orders)
                {
                    await _httpClient.DeleteAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/order/{order.RowKey}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
                }

                return RedirectToAction("CheckoutOrders");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during checkout: {ex.Message}");
                return View("Error");
            }
        }

        public async Task<IActionResult> CheckoutOrders()
        {
            try
            {
                // Get the customerId (likely the user's email or unique identifier)
                var customerId = User.FindFirstValue(ClaimTypes.Email);

                // Construct the Azure Function URL
                var functionUrl = $"https://cloudfunctionappst10373357.azurewebsites.net/api/checkoutOrders/customer/{customerId}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D";

                // Make an HTTP GET request to the Azure Function
                var response = await _httpClient.GetAsync(functionUrl);

                // If the request was successful
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<CheckoutOrder>>(responseBody);

                    // Return the orders to the view
                    return View(orders);
                }
                else
                {
                    // Handle error if the function returns a bad status code
                    return StatusCode((int)response.StatusCode, "Error fetching orders from Azure Function");
                }
            }
            catch (Exception ex)
            {
                // Log and return the error view if something goes wrong
                Console.WriteLine($"Error fetching checkout orders: {ex.Message}");
                return View("Error");
            }
        }
        }
}
