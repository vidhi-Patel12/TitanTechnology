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

        // List
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

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
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

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

            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var checkResponse = await client.GetAsync($"{_apiUrl}/{model.ProjectCode}");
            if (checkResponse.IsSuccessStatusCode && string.IsNullOrEmpty(model.ProjectCode) == false)
            {
                ModelState.AddModelError("ProjectCode", "Project Code already exists!");
                return View("AddEdit", model);
            }

            model.ProjectCode = model.ProjectCode?.Trim();

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

        [HttpGet]
        public async Task<IActionResult> Details(string projectCode)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            // 2️⃣ Fallback: Fetch Project only
            var projectResponse = await client.GetAsync($"{_apiUrl}/{projectCode}");
            if (!projectResponse.IsSuccessStatusCode)
            {
                return NotFound("Project not found");
            }

            var projectJson = await projectResponse.Content.ReadAsStringAsync();
            var projectMaster = JsonConvert.DeserializeObject<ProjectMaster>(projectJson);

            // 3️⃣ Fetch Employees separately for this project
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/ProjectEmployee/by-project/{projectCode}");
            var projectEmployees = new List<ProjectEmployee>();

            if (employeeResponse.IsSuccessStatusCode)
            {
                var empJson = await employeeResponse.Content.ReadAsStringAsync();
                projectEmployees = JsonConvert.DeserializeObject<List<ProjectEmployee>>(empJson) ?? new List<ProjectEmployee>();
            }

            // 3️⃣ Fetch all Employees to get names
            var allEmployeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");
            var allEmployees = new List<EmployeeFormDto>();
            if (allEmployeeResponse.IsSuccessStatusCode)
            {
                var empJson = await allEmployeeResponse.Content.ReadAsStringAsync();
                allEmployees = JsonConvert.DeserializeObject<List<EmployeeFormDto>>(empJson) ?? new List<EmployeeFormDto>();
            }

            foreach (var pe in projectEmployees)
            {
                var emp = allEmployees.FirstOrDefault(e => e.EmployeeId == pe.EmployeeId);
                if (emp != null)
                {
                    pe.Name = emp.Name;
                }
            }

            // 🔹 Fetch Customer for this project
            if (projectMaster != null && projectMaster.CustomerId > 0)
            {
                var customerResponse = await client.GetAsync($"{_apiOrigin}/api/Customer/{projectMaster.CustomerId}");
                if (customerResponse.IsSuccessStatusCode)
                {
                    var customerJson = await customerResponse.Content.ReadAsStringAsync();
                    var customer = JsonConvert.DeserializeObject<CustomerFormDto>(customerJson);

                    if (customer != null)
                    {
                        projectMaster.CustomerMaster = new CustomerMaster
                        {
                            CustomerId = customer.CustomerId,
                            CustomerName = customer.CustomerName
                        };
                    }
                }
            }

            // 4️⃣ Build DTO manually
            var res= new ProjectEmployeeMasterDto
            {
                projectMasters = projectMaster,
                projectEmployees = projectEmployees
            };

            return View("ViewProjectMasterEmployee", res);
        }

        [HttpGet]
        public async Task<IActionResult> CheckProjectCode(string projectCode)
        {
            if (string.IsNullOrEmpty(projectCode))
                return Json(new { exists = false });

            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.GetAsync($"{_apiUrl}/{projectCode}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { exists = true });
            }
            return Json(new { exists = false });
        }


        // Delete
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
