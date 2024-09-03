using Admin_CBMS.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Admin_CBMS.Controllers;
public class ProductsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ImportProducts(IFormFile file)
    {
        string pathToUpload = $"{Directory.GetCurrentDirectory()}\\files\\{file.FileName}";

        using (FileStream fileStream = System.IO.File.Create(pathToUpload))
        {
            file.CopyTo(fileStream);
            fileStream.Flush();
        }

        List<Product> products = GetAllProductsFromFile(file.FileName);
        HttpClient client = new HttpClient();
        string URL = "https://localhost:7248/api/Admin/ImportAllProducts";

        HttpContent content = new StringContent(JsonConvert.SerializeObject(products), Encoding.UTF8, "application/json");

        HttpResponseMessage response = client.PostAsync(URL, content).Result;

        var result = response.Content.ReadAsAsync<bool>().Result;

        return RedirectToAction("Index", "Order");
    }

    private static List<Product> GetAllProductsFromFile(string fileName)
    {
        List<Product> products = new List<Product>();
        string filePath = $"{Directory.GetCurrentDirectory()}\\files\\{fileName}";

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        Name = reader.GetValue(0).ToString(),
                        Price = int.Parse(reader.GetValue(1).ToString()),
                        Category = reader.GetValue(2).ToString(),
                        Quantity = int.Parse(reader.GetValue(3).ToString()),
                    });
                }
            }
        }
        return products;

    }
}
