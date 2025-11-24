using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using TitanTechnologyView.Extensions;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class ProjectMasterController : Controller
    {       
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly string _apiOrigin;

        public ProjectMasterController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = $"{apiSettings.Value.BaseUrl}/ProjectMaster";
            _apiOrigin = apiSettings.Value.Origin;
        }

        private HttpClient CreateClients()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            // Fetch JWT token from cookie
            var token = HttpContext.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                //  Add Bearer token to Authorization header
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("Warning: AuthToken cookie not found!");
            }

            return client;
        }

        // List
        public async Task<IActionResult> Index()
        {
            var client = CreateClients();

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
            var client = CreateClients();

            var customerResponse = await client.GetAsync($"{_apiOrigin}/api/Customer");
            var customers = new List<CustomerFormDto>();

            if (customerResponse.IsSuccessStatusCode)
            {
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerFormDto>>(customerJson) ?? new List<CustomerFormDto>();
            }
            ViewBag.Customers = customers;

            var approvalLevel = await client.GetDropdownAsync(_apiOrigin, "Timesheet Approval Level");
            ViewBag.TimesheetApprovalLevel = approvalLevel;

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
        public async Task<IActionResult> Save(ProjectMaster model, bool? RedirectToEmployee)
        {
            if (!ModelState.IsValid)
            {
                return View("AddEdit", model);
            }
            model.ProjectCode = model.ProjectCode?.Trim();
            model.Status = "Active";

            var client = CreateClients();
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                if (RedirectToEmployee == true)
                {
                    // redirect to ProjectEmployee Add page with ProjectCode
                    return RedirectToAction("AddEdit", "ProjectEmployee", new { projectCode = model.ProjectCode });
                }
                return RedirectToAction("Index");
            }
           
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("AddEdit", model);
        }

        // Delete
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var client = CreateClients();
            var response = await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
