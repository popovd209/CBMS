using Admin_CBMS.Models.WaiterPerformance;
using Admin_CBMS.Models.WaiterPerformance.DTOs;
using Entity.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace Admin_CBMS.Controllers;

    public class WaiterPerformanceController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var pvm = new WaiterPerformanceViewModel
            {
                SelectedWaiterId = "0",
                SelectedDate = DateTime.Now,
                Waiters = new SelectList(Enumerable.Empty<SelectListItem>()),
                PerformanceResults = new List<PerformanceResult>()
            };

            string URL = "https://localhost:7248/api/Admin/GetUsersByRole";
            HttpClient client = new HttpClient();

            var model = new
            {
                Role = "WAITER"
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(URL, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ICollection<Waiter>>(jsonString);
                
                return View(pvm);
            }
            else
            {
                // Handle error response
                return View(pvm); // Empty list on error
            }
        }


        //    public async Task<IActionResult> Search(WaiterPerformanceViewModel model)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return View("Index", model); // Reload the form if the model is invalid
        //        }

        //        // Construct the API request URL with query parameters
        //        string apiUrl = $"https://localhost:7248/api/Admin/GetWaiterPerformance?waiterId={model.SelectedWaiterId}&date={model.SelectedDate.ToString("yyyy-MM-dd")}";
        //        HttpClient client = new HttpClient();
        //        HttpResponseMessage response = await client.GetAsync(apiUrl);

        //        // Fetch the performance results from the API
        //        var performanceData = await response.Content.ReadAsAsync<List<PerformanceResult>>();

        //        model.PerformanceResults = performanceData;  // Set results in the model

        //        return View("Index", model); // Reload the view with the updated performance data
        //    }
}
