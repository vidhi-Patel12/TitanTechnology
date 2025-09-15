using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using TitanTechnologyView.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TitanTechnologyView.Controllers
{
    public class ProjectMasterController : Controller
    {
        private readonly string _apiOrigin = "https://localhost:44368";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl = "https://localhost:44368/api/ProjectMaster";

        public ProjectMasterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // List
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();

            // 1️⃣ Fetch projects
            var projectResponse = await client.GetAsync(_apiUrl);
            var projects = new List<ProjectMaster>();
            if (projectResponse.IsSuccessStatusCode)
            {
                var json = await projectResponse.Content.ReadAsStringAsync();
                projects = JsonConvert.DeserializeObject<List<ProjectMaster>>(json) ?? new List<ProjectMaster>();
            }

            // 2️⃣ Fetch customers
            var customerResponse = await client.GetAsync($"{_apiOrigin}/api/Customer");
            var customers = new List<CustomerFormDto>();
            if (customerResponse.IsSuccessStatusCode)
            {
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerFormDto>>(customerJson) ?? new List<CustomerFormDto>();
            }

            // 3️⃣ Attach CustomerName to each project
            foreach (var project in projects)
            {
                var customer = customers.FirstOrDefault(c => c.CustomerId == project.CustomerId);
                if (customer != null)
                {
                    project.CustomerMaster = new CustomerMaster
                    {
                        CustomerId = customer.CustomerId,
                        CustomerName = customer.CustomerName
                    };
                }
            }

            return View(projects);
        }


        // Add / Edit form
        [HttpGet]
        public async Task<IActionResult> AddEdit(string? projectCode)
        {
            ViewBag.ApiOrigin = _apiOrigin;
            var client = _httpClientFactory.CreateClient();

            var customerResponse = await client.GetAsync($"{_apiOrigin}/api/Customer");
            var customers = new List<CustomerFormDto>();

            if (customerResponse.IsSuccessStatusCode)
            {
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerFormDto>>(customerJson) ?? new List<CustomerFormDto>();
            }
            ViewBag.Customers = customers;

            if (string.IsNullOrEmpty(projectCode))
            {
                return View(new ProjectMaster()); // Add form
            }

            var response = await client.GetAsync($"{_apiUrl}/{projectCode}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<ProjectMaster>(json) ?? new ProjectMaster();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(ProjectMaster model)
        {

            if (!ModelState.IsValid)
            {
                return View("AddEdit", model);
            }

            model.ProjectCode = model.ProjectCode?.Trim();

            var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("AddEdit", model);
        }


        // Delete
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
