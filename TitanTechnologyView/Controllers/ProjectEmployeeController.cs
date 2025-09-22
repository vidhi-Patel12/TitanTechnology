using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using TitanTechnologyView.Extensions;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class ProjectEmployeeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly string _apiOrigin;

        public ProjectEmployeeController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = $"{apiSettings.Value.BaseUrl}/ProjectEmployee";
            _apiOrigin = apiSettings.Value.Origin;
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
        public async Task<IActionResult> AddEdit(int? id, string? projectCode)
            {
            ViewBag.ApiOrigin = _apiOrigin;
            var client = _httpClientFactory.CreateClient();

            // Fetch projects
            var projectResponse = await client.GetAsync($"{_apiOrigin}/api/ProjectMaster");
            var projects = new List<ProjectMaster>();
            if (projectResponse.IsSuccessStatusCode)
            {
                var projectJson = await projectResponse.Content.ReadAsStringAsync();
                projects = JsonConvert.DeserializeObject<List<ProjectMaster>>(projectJson) ?? new();
            }
            ViewBag.Projects = projects;

            // Fetch employees
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");
            var employees = new List<EmployeeFormDto>();
            if (employeeResponse.IsSuccessStatusCode)
            {
                var employeeJson = await employeeResponse.Content.ReadAsStringAsync();
                employees = JsonConvert.DeserializeObject<List<EmployeeFormDto>>(employeeJson) ?? new();
            }
            
            ViewBag.Employees = employees;

            var technologys = await client.GetDropdownAsync(_apiOrigin, "Technology");
            ViewBag.Technologys = technologys;

            var employeeTypes = await client.GetDropdownAsync(_apiOrigin, "Employee Type");
            ViewBag.EmployeeTypes = employeeTypes;

            var allocationTypes = await client.GetDropdownAsync(_apiOrigin, "Allocation Type");
            ViewBag.AllocationTypes = allocationTypes;

            var timesheetTypes = await client.GetDropdownAsync(_apiOrigin, "Timesheet Type");
            ViewBag.TimesheetTypes = timesheetTypes;

            var sapModules = await client.GetDropdownAsync(_apiOrigin, "SAP Module");
            ViewBag.SAPModules = sapModules;

            var modeofPayments = await client.GetDropdownAsync(_apiOrigin, "Mode of Payment");
            ViewBag.ModeofPayments = modeofPayments;

            // Prepare model
            List<ProjectEmployee> model;

            if (id.HasValue)
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Could not load Project Employee.";
                    return RedirectToAction("Index");
                }
                var data = JsonConvert.DeserializeObject<ProjectEmployee>(await response.Content.ReadAsStringAsync());
                model = new List<ProjectEmployee> { data };
            }
            else
            {
                var newEmployee = new ProjectEmployee();
                if (!string.IsNullOrEmpty(projectCode))
                    newEmployee.ProjectCode = projectCode;

                model = new List<ProjectEmployee> { newEmployee };
            }

            return View(model); // now passes List<ProjectEmployee>

        }

        // POST: Save
        [HttpPost]
        public async Task<IActionResult> Save(List<ProjectEmployee> model)
        {
            foreach (var employee in model) {
                if (employee.EmployeeStartDate.HasValue && employee.EmployeeEndDate.HasValue &&
                    employee.EmployeeEndDate < employee.EmployeeStartDate)
                {                    
                    ModelState.AddModelError("EmployeeEndDate_{employee.Id}", "End Date cannot be earlier than Start Date.");                    
                }

                if (employee.CycleStartDay.HasValue && employee.CycleEndDay.HasValue &&
                    employee.CycleEndDay < employee.CycleStartDay)
                {                    
                    ModelState.AddModelError("CycleEndDay_{employee.Id}", "Cycle End Day cannot be earlier than Cycle Start Day.");                    
                }
            }
            // === Cycle Day Validation ===
            
            if (!ModelState.IsValid) 
            { 
                return View("AddEdit", model);
            }

            // Ensure booleans post properly
            
            var client = _httpClientFactory.CreateClient();

            foreach (var employee in model) 
            {
                //employee.Inactive = employee.Inactive ?? true;
                employee.TimesheetRequired = employee.TimesheetRequired ?? true;

                var jsonData = JsonConvert.SerializeObject(employee);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (employee.Id == 0)
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

            }    // Save successful → set flag
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
