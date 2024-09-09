using Admin_CBMS.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Admin_CBMS.Controllers;

public class ProductsController : Controller
{
    public async Task<IActionResult> Index()
    {
        string URL = "https://cbms.azurewebsites.net/api/Admin/GetAllProducts";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = client.GetAsync(URL).Result;

        var result = response.Content.ReadAsAsync<ICollection<Product>>().Result;

        return View(result);
    }

    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        Stream stream = new MemoryStream();
        await file.CopyToAsync(stream);

        List<Product> products = GetProductsFromFile(stream);

        HttpClient client = new HttpClient();
        string URL = "https://cbms.azurewebsites.net/api/Admin/ImportProducts";

        HttpContent content = new StringContent(JsonConvert.SerializeObject(products), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(URL, content);

        return RedirectToAction("Index");
    }

    private static List<Product> GetProductsFromFile(Stream stream)
    {
        List<Product> products = new List<Product>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(stream);
        while (reader.Read())
        {
            products.Add(new Product
            {
                Name = reader.GetValue(0).ToString() ?? "product name not provided",
                Price = int.Parse(reader.GetValue(1).ToString() ?? "0"),
                Category = reader.GetValue(2).ToString() ?? "product category not provided",
                Quantity = int.Parse(reader.GetValue(3).ToString() ?? "0"),
            });
        }

        return products;

    }
}
