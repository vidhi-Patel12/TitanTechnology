using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class ProjectEmployeeController : Controller
    {
        private readonly string _apiOrigin = "https://localhost:44368";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:44368/api/ProjectEmployee"; // API Base URL

        public ProjectEmployeeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: List
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(_apiBaseUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load data.";
                return View(new List<ProjectEmployee>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<ProjectEmployee>>(json) ?? new List<ProjectEmployee>();

            // Fetch employees
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");
            var employees = new List<EmployeeFormDto>();
            if (employeeResponse.IsSuccessStatusCode)
            {
                var employeeJson = await employeeResponse.Content.ReadAsStringAsync();
                employees = JsonConvert.DeserializeObject<List<EmployeeFormDto>>(employeeJson) ?? new();
            }

            // Map EmployeeId -> Name
            var employeeDict = employees.ToDictionary(e => e.EmployeeId, e => e.Name);

            foreach (var datas in data)
            {
                datas.Name = employeeDict.TryGetValue(datas.EmployeeId, out var name) ? name : "N/A";
            }

            return View(data);
        }


        // GET: Add/Edit
        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.ApiOrigin = _apiOrigin;            
            var client = _httpClientFactory.CreateClient();

            var projectResponse = await client.GetAsync($"{_apiOrigin}/api/ProjectMaster");
            var projects = new List<ProjectMaster>();
            if (projectResponse.IsSuccessStatusCode)
            {
                var projectJson = await projectResponse.Content.ReadAsStringAsync();
                projects = JsonConvert.DeserializeObject<List<ProjectMaster>>(projectJson) ?? new();
            }
            ViewBag.Projects = projects;


            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");

            var employees = new List<EmployeeFormDto>();

            if (employeeResponse.IsSuccessStatusCode)
            {
                var employeeJson = await employeeResponse.Content.ReadAsStringAsync();
                employees = JsonConvert.DeserializeObject<List<EmployeeFormDto>>(employeeJson) ?? new List<EmployeeFormDto>();
            }

            ViewBag.Employees = employees;

            if (id == null) return View(new ProjectEmployee());

            var response = await client.GetAsync($"{_apiBaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not load Project Employee.";
                return RedirectToAction("Index");
            }

            var data = JsonConvert.DeserializeObject<ProjectEmployee>(await response.Content.ReadAsStringAsync());

            var employee = employees.FirstOrDefault(e => e.EmployeeId == data.EmployeeId);

            if (employee != null)
            {
                ViewBag.SelectedEmployeeName = employee.Name;

            }
            return View(data);
        }

        // POST: Save
        [HttpPost]
        public async Task<IActionResult> Save(ProjectEmployee model)
        {
            if (model.EmployeeStartDate.HasValue && model.EmployeeEndDate.HasValue)
            {
                if (model.EmployeeEndDate < model.EmployeeStartDate)
                {
                    ModelState.AddModelError("EmployeeEndDate", "End Date cannot be earlier than Start Date.");
                }
            }


            // === Cycle Day Validation ===
            if (model.CycleStartDay.HasValue && model.CycleEndDay.HasValue)
            {
                if (model.CycleEndDay < model.CycleStartDay)
                {
                    ModelState.AddModelError("CycleEndDay", "Cycle End Day cannot be earlier than Cycle Start Day.");
                }             
            }

            if (!ModelState.IsValid) 
            { 
                return View("AddEdit", model);
            }

            // Ensure booleans post properly
            model.Inactive = model.Inactive ?? true;
            model.TimesheetRequired = model.TimesheetRequired ?? true;

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (model.Id == null || model.Id == 0 )
            {
                response = await client.PostAsync(_apiBaseUrl, content);
            }
            else
            {
                response = await client.PostAsync(_apiBaseUrl, content);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errors = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"API Error: {errors}");

                TempData["Error"] = "Save failed!";
                return View("AddEdit", model);
            }

            TempData["Success"] = "Saved successfully!";
            return RedirectToAction("Index");
        }


        // DELETE
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Delete failed!";
            else
                TempData["Success"] = "Deleted successfully!";

            return RedirectToAction("Index");
        }
    }
}
