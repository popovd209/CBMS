using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using Admin_CBMS.Views.Waiters;
using Admin_CBMS.Views.Waiters.DTOs;

namespace Admin_CBMS.Controllers
{
    public class WaitersController : Controller
    {
        public async Task<IActionResult> Index()
        {
            string URL = "https://localhost:7248/api/Admin/GetUserWithRole";
            HttpClient client = new HttpClient();

            var modelWaiter = new
            {
                role = "WAITER"
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(modelWaiter), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var waiters = response.Content.ReadAsAsync<ICollection<WaiterDTO>>().Result;

            var pvm = new WaiterPerformanceViewModel
            {
                SelectedWaiterId = "0",
                SelectedDate = DateTime.Now,
                Waiters = new SelectList(waiters, "Id", "DisplayName"),
                PerformanceResult = new PerformanceResultDTO()
            };

            return View(pvm);
        }

        public async Task<IActionResult> Search(string SelectedWaiterId, DateTime SelectedDate)
        {
            //Change url
            string URL = "https://localhost:7248/api/Admin/GetUserWithRole";
            HttpClient client = new HttpClient();

            var modelSearch = new
            {
                Id = SelectedWaiterId,
                Date = SelectedDate
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(modelSearch), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var waiterResult = response.Content.ReadAsAsync<PerformanceResultDTO>().Result;



            return RedirectToAction("Index");
        }
    }

}