using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using Admin_CBMS.Views.Waiters;
using Admin_CBMS.Views.Waiters.DTOs;
using GemBox.Document;

namespace Admin_CBMS.Controllers
{
    public class WaitersController : Controller
    {
        public WaitersController()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        }
        public async Task<IActionResult> Index()
        {
            string URL = "https://cbms.azurewebsites.net/api/Admin/GetUserWithRole";
            HttpClient client = new HttpClient();

            var modelWaiter = new
            {
                role = "WAITER"
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(modelWaiter), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var waiters = response.Content.ReadAsAsync<ICollection<WaiterDTO>>().Result;

            PerformanceResultDTO performanceResult = new PerformanceResultDTO();
            try
            {
                if (TempData["PerformanceResult"] != null)
                {
                    performanceResult = JsonConvert.DeserializeObject<PerformanceResultDTO>(TempData["PerformanceResult"].ToString()) ?? new PerformanceResultDTO();
                }
            }
            catch (JsonReaderException)
            {
                performanceResult = new PerformanceResultDTO();
            }

            var pvm = new WaiterPerformanceViewModel
            {
                SelectedWaiterId = TempData["SelectedWaiterId"]?.ToString() ?? "0",
                SelectedDate = TempData["SelectedDate"] != null ? (DateTime)TempData["SelectedDate"] : DateTime.Now,
                Waiters = new SelectList(waiters, "Id", "DisplayName"),
                PerformanceResult = performanceResult
            };
            return View(pvm);
        }

        public async Task<IActionResult> Search(string SelectedWaiterId, DateTime SelectedDate)
        {
            string URL = "https://cbms.azurewebsites.net/api/Admin/GetPerformanceByWaiterAndDate";
            HttpClient client = new HttpClient();
            
            var modelSearch = new
            {
                Id = SelectedWaiterId,
                Date = SelectedDate
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(modelSearch), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(URL, content);

            var waiterResult = await response.Content.ReadAsAsync<PerformanceResultDTO>();

            // Store the performance result and selected waiter in TempData to pass it to Index
            if (waiterResult == null || (waiterResult.TotalOrdersServed == 0 && waiterResult.TotalIncome == 0))
            {
                TempData["PerformanceResult"] = "No data found for the selected waiter and date.";
            }
            else
            {
                TempData["PerformanceResult"] = JsonConvert.SerializeObject(waiterResult);
            }
            TempData["SelectedWaiterId"] = SelectedWaiterId;
            TempData["SelectedDate"] = SelectedDate;

            return RedirectToAction("Index");
        }

        public FileContentResult GenerateReport(
            string WaiterId, string WaiterName, 
            int? TotalOrdersServed, string MostCommonCategory,
            double? TotalIncome, DateTime? Date)
        {
            HttpClient client = new HttpClient();

            string baseURL = "https://cbms.azurewebsites.net/api/Admin/GetUserWithId";
            string queryString = $"id={WaiterId}"; 
            string URL = $"{baseURL}?{queryString}";

            HttpResponseMessage response = client.GetAsync(URL).Result;
            var waiter = response.Content.ReadAsAsync<WaiterDTO>().Result;

            var templatePath = Path.Combine(AppContext.BaseDirectory, "files", "WaiterReportTemplate.docx");

            var document = DocumentModel.Load(templatePath);

            document.Content.Replace("{{WaiterId}}", WaiterId);
            document.Content.Replace("{{WaiterName}}", WaiterName);
            document.Content.Replace("{{WaiterEmail}}", waiter.Email ?? "");
            document.Content.Replace("{{PersonalPin}}", waiter.PersonalPin ?? "");
            document.Content.Replace("{{ContractDate}}", waiter.ContractDate.Value.ToShortDateString() ?? "");

            document.Content.Replace("{{Date}}", Date != null ? Date.Value.ToShortDateString() : "");

            document.Content.Replace("{{TotalOrdersServed}}", TotalOrdersServed != null ? TotalOrdersServed.Value.ToString() : "");
            document.Content.Replace("{{MostCommonCategory}}", MostCommonCategory);
            document.Content.Replace("{{TotalIncome}}", TotalIncome != null ? TotalIncome.Value.ToString() + " MKD" : "");

            var stream = new MemoryStream();
            document.Save(stream, new PdfSaveOptions());
            return File(stream.ToArray(), new PdfSaveOptions().ContentType, $"{WaiterId}_PerformanceReport.pdf");
        }

    }

}