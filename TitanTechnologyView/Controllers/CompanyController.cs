using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class CompanyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public CompanyController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = $"{apiSettings.Value.BaseUrl}/Company";
        }

        // GET: /Company
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.GetAsync(_apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "API call failed: " + response.StatusCode;
                return View(new List<CompanyMaster>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var companies = JsonConvert.DeserializeObject<List<CompanyMaster>>(json);

            return View(companies);

        }

        // GET: /Company/AddCompany/{code?}

        [HttpGet]
        public async Task<IActionResult> AddCompany(string? id = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(new CompanyMaster()); // add
            }

            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.GetAsync($"{_apiUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var company = JsonConvert.DeserializeObject<CompanyMaster>(json);
            return View(company); // edit
        }

        // POST: /Company/AddCompany
        [HttpPost]
        public async Task<IActionResult> AddCompany(CompanyMaster model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API InsertUpdate handles both add & update
            var response = await client.PostAsync(_apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View(model);
        }

        // GET: /Company/Delete/{code}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
