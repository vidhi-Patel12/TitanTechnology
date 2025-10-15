using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TitanTechnologyView.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TitanTechnologyView.Controllers
{
    public class TimesheetController : Controller
    {        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiBaseUrl;
        private readonly string _apiOrigin;

        public TimesheetController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            apiBaseUrl = $"{apiSettings.Value.BaseUrl}/Timesheet";
            _apiOrigin = apiSettings.Value.Origin;
        }

        // Show Add/Edit Form
        public async Task<IActionResult> AddEditAsync(int? id)
        {
            ViewBag.ApiOrigin = _apiOrigin;
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var projects = new List<ProjectMaster>();
            var projectResponse = await client.GetAsync($"{_apiOrigin}/api/ProjectMaster");
            if (projectResponse.IsSuccessStatusCode)
            {
                projects = await projectResponse.Content.ReadFromJsonAsync<List<ProjectMaster>>() ?? new();
            }
            ViewBag.Projects = projects;

            var employees = new List<EmployeeFormDto>();
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");

            if (employeeResponse.IsSuccessStatusCode)
            {
                employees = await employeeResponse.Content.ReadFromJsonAsync<List<EmployeeFormDto>>() ?? new();
            }

            var employeeDisplayList = employees.Select(e => new
            {
                EmployeeId = e.EmployeeId,
                DisplayName = $" {e.Name} ({e.EmployeeId}) "
            }).ToList();

            ViewBag.Employees = employeeDisplayList;

            Timesheet model = new Timesheet();
            if (id.HasValue && id > 0)
            {
                var response = await client.GetAsync($"{_apiOrigin}/api/Timesheet/{id}");
                if (response.IsSuccessStatusCode)
                {
                    model = await response.Content.ReadFromJsonAsync<Timesheet>() ?? new Timesheet();
                }
            }

            return View(model);
        }

        // Handle Save (Add or Update)
        [HttpPost]
        public async Task<IActionResult> SaveTimesheet(Timesheet model)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var allTimesheetsResponse = await client.GetAsync(apiBaseUrl);
            if (allTimesheetsResponse.IsSuccessStatusCode)
            {
                var allTimesheets = await allTimesheetsResponse.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();

                bool exists = allTimesheets.Any(ts =>
                    ts.ProjectCode == model.ProjectCode &&
                    ts.EmployeeId == model.EmployeeId &&
                    ts.TimesheetType == model.TimesheetType &&
                    ts.MonthYear == model.MonthYear &&
                    ts.TimesheetId != model.TimesheetId // allow update of same record
                );

                if (exists)
                {
                    ModelState.AddModelError("", "Timesheet already exists for this Project, Employee, Type and Month/Year.");
                    await LoadDropdownData();
                    return View("AddEdit", model);
                }
            }

            // ✅ Auto set StartDate & EndDate from MonthYear
            // ✅ Auto set StartDate & EndDate from MonthYear and TimesheetType
            if (!string.IsNullOrEmpty(model.MonthYear) &&
                DateTime.TryParseExact(model.MonthYear, "yyyy-MM", null,
                    System.Globalization.DateTimeStyles.None, out var parsedMonthYear))
            {
                model.StartDate = new DateTime(parsedMonthYear.Year, parsedMonthYear.Month, 1);

                switch (model.TimesheetType)
                {
                    case "Monthly":
                        model.EndDate = new DateTime(parsedMonthYear.Year, parsedMonthYear.Month,
                                                     DateTime.DaysInMonth(parsedMonthYear.Year, parsedMonthYear.Month));
                        break;

                    case "Quarterly":
                        // Add 3 months, subtract 1 day to get the last day of the 3rd month
                        var endMonth = parsedMonthYear.Month + 2;
                        var endYear = parsedMonthYear.Year;

                        // Handle year change if month > 12
                        if (endMonth > 12)
                        {
                            endMonth -= 12;
                            endYear += 1;
                        }

                        model.EndDate = new DateTime(endYear, endMonth,
                                                     DateTime.DaysInMonth(endYear, endMonth));
                        break;

                    case "Fixed":
                    case "Project":
                    default:
                        model.EndDate = new DateTime(parsedMonthYear.Year, parsedMonthYear.Month,
                                                     DateTime.DaysInMonth(parsedMonthYear.Year, parsedMonthYear.Month));
                        break;
                }
            }

            // ✅ Post to API
            string jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiBaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            else
            {
                ModelState.AddModelError("", "Error saving data.");
                await LoadDropdownData();
                return View("AddEdit", model);
            }
        }

        // Show List Page
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            // 1. Get Timesheets
            var timesheets = new List<Timesheet>();
            var timesheetResponse = await client.GetAsync($"{_apiOrigin}/api/Timesheet");
            if (timesheetResponse.IsSuccessStatusCode)
            {
                timesheets = await timesheetResponse.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();
            }

            // 2. Get Employees
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");
            var employees = new List<EmployeeFormDto>();
            if (employeeResponse.IsSuccessStatusCode)
            {
                employees = await employeeResponse.Content.ReadFromJsonAsync<List<EmployeeFormDto>>() ?? new();
            }

            // 3. Create dictionary for EmployeeId -> Name
            var employeeDict = employees.ToDictionary(e => e.EmployeeId, e => e.Name);

            // 4. Map EmployeeId to EmployeeName inside timesheets
            foreach (var ts in timesheets)
            {
                ts.Name = employeeDict.TryGetValue(ts.EmployeeId, out var name) ? name : "N/A";
            }

            return View(timesheets);
        }


        public async Task<IActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                var response = client.DeleteAsync($"{apiBaseUrl}/{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        private async Task LoadDropdownData()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var projects = new List<ProjectMaster>();
            var projectResponse = await client.GetAsync($"{_apiOrigin}/api/ProjectMaster");
            if (projectResponse.IsSuccessStatusCode)
            {
                projects = await projectResponse.Content.ReadFromJsonAsync<List<ProjectMaster>>() ?? new();
            }
            ViewBag.Projects = projects;

            var employees = new List<EmployeeFormDto>();
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");
            if (employeeResponse.IsSuccessStatusCode)
            {
                employees = await employeeResponse.Content.ReadFromJsonAsync<List<EmployeeFormDto>>() ?? new();

            }
            ViewBag.Employees = employees;
        }

        [HttpGet]
        public async Task<IActionResult> CheckDuplicate(string projectCode, int employeeId, string timesheetType, string monthYear, int? timesheetId)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var allTimesheetsResponse = await client.GetAsync(apiBaseUrl);
            if (allTimesheetsResponse.IsSuccessStatusCode)
            {
                var allTimesheets = await allTimesheetsResponse.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();

                bool exists = allTimesheets.Any(ts =>
                    ts.ProjectCode == projectCode &&
                    ts.EmployeeId == employeeId &&
                    ts.TimesheetType == timesheetType &&
                    ts.MonthYear == monthYear &&
                    ts.TimesheetId != timesheetId);

                if (exists)
                {
                    return Json(new { isDuplicate = true });
                }
            }

            return Json(new { isDuplicate = false });
        }
    }
}
