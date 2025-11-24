using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class TimesheetEntryController : Controller
    {
        private readonly string _apiOrigin = "https://api.titentechnology.com";
        private readonly string apiBaseUrl = "https://api.titentechnology.com/api/TimesheetEntry";
        private readonly IHttpClientFactory _httpClientFactory;

        public TimesheetEntryController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Show Add/Edit Form
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.ApiOrigin = _apiOrigin;
            await LoadTimesheets(); // fill dropdown
            var model = new TimeSheetEntry();

            // If editing
            if (id is > 0)
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{apiBaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<TimeSheetEntry>(json) ?? new TimeSheetEntry();
                }
                else
                {
                    TempData["Error"] = $"Could not load TimesheetEntry with ID {id}";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        // Handle Save (Add or Update)
        [HttpPost]
        public async Task<IActionResult> SaveEntryAsync(TimeSheetEntry model)
        {
            if (!ModelState.IsValid)
            {
                await LoadTimesheets();
                return View("AddEdit", model);
            }

            var client = _httpClientFactory.CreateClient();

            // 🔍 Load all entries to check duplicates
            var allResponse = await client.GetAsync(apiBaseUrl);
            if (!allResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error fetching existing entries");
                await LoadTimesheets();
                return View("AddEdit", model);
            }

            var allJson = await allResponse.Content.ReadAsStringAsync();
            var allEntries = JsonConvert.DeserializeObject<List<TimeSheetEntry>>(allJson) ?? new();

            // ✅ Duplicate check only when adding new
            var existingEntry = allEntries.FirstOrDefault(e =>
                e.TimesheetId == model.TimesheetId &&
                e.EntryDate.HasValue &&
                model.EntryDate.HasValue &&
                e.EntryDate.Value.Date == model.EntryDate.Value.Date);

            if (existingEntry != null )
            {
                // Just reject silently (frontend already checks)
                model.EntryId = existingEntry.EntryId;
            }

            string jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (model.EntryId == 0) // Add
            {
                response = client.PostAsync(apiBaseUrl, content).Result;
            }
            else // Update
            {
                response = client.PostAsync(apiBaseUrl, content).Result;
            }

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Error saving data");
                await LoadTimesheets();
                return View("AddEdit", model);
            }
            
        }

        // Show List Page
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var list = new List<TimeSheetEntry>();

            var response = await client.GetAsync(apiBaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<TimeSheetEntry>>(json) ?? new();
            }

            // 🔹 Load Timesheets & Employees to enrich data
            var timesheetResponse = await client.GetAsync($"{_apiOrigin}/api/Timesheet");
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");

            var timesheets = timesheetResponse.IsSuccessStatusCode
                ? await timesheetResponse.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new()
                : new List<Timesheet>();

            var employees = employeeResponse.IsSuccessStatusCode
                ? await employeeResponse.Content.ReadFromJsonAsync<List<EmployeeMaster>>() ?? new()
                : new List<EmployeeMaster>();

            // 🔹 Enrich DisplayText for each entry
            foreach (var entry in list)
            {
                var ts = timesheets.FirstOrDefault(t => t.TimesheetId == entry.TimesheetId);
                var emp = ts != null ? employees.FirstOrDefault(e => e.EmployeeId == ts.EmployeeId) : null;

                //if (ts != null && emp != null)
                //{
                //    entry.DisplayText = $"{ts.ProjectCode} - {emp.Name} - {ts.MonthYear:MM/yyyy} - {ts.TimesheetType}";
                //}
                //else
                //{
                //    entry.DisplayText = $"Timesheet #{entry.TimesheetId}";
                //}
            }

            return View(list);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{apiBaseUrl}/{id}");
            return RedirectToAction("Index");
        }

        private async Task LoadTimesheets()
        {
            var client = _httpClientFactory.CreateClient();
            var timesheets = new List<Timesheet>();
            var employees = new List<EmployeeMaster>();

            var timesheetResponse = await client.GetAsync($"{_apiOrigin}/api/Timesheet");
            if (timesheetResponse.IsSuccessStatusCode)
            {
                timesheets = await timesheetResponse.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();
            }

            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");
            if (employeeResponse.IsSuccessStatusCode)
            {
                employees = await employeeResponse.Content.ReadFromJsonAsync<List<EmployeeMaster>>() ?? new();
            }

            // Build display label: "ProjectCode - EmployeeName - MonthYear"
            var timesheetItems = (from ts in timesheets
                                  join emp in employees on ts.EmployeeId equals emp.EmployeeId
                                  select new
                                  {
                                      TimesheetId = ts.TimesheetId,
                                      //DisplayText = $"{ts.ProjectCode} - {emp.Name} - {ts.MonthYear:MM/yyyy} - {ts.TimesheetType}",
                                  //    StartDate = ts.StartDate.HasValue ? ts.StartDate.Value.ToString("yyyy-MM-dd") : "",
                                  //    EndDate = ts.EndDate.HasValue ? ts.EndDate.Value.ToString("yyyy-MM-dd") : ""
                                 }).ToList();

            ViewBag.Timesheets = new SelectList(timesheetItems, "TimesheetId", "DisplayText");
            //ViewBag.TimesheetDates = timesheetItems.ToDictionary(x => x.TimesheetId, x => new { x.StartDate, x.EndDate });
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> CheckDuplicate(int timesheetId, DateTime entryDate)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(apiBaseUrl);

            if (!response.IsSuccessStatusCode)
                return Json(new { exists = false });

            var json = await response.Content.ReadAsStringAsync();
            var allEntries = JsonConvert.DeserializeObject<List<TimeSheetEntry>>(json) ?? new();

            // Check if an entry exists for this TimesheetId + EntryDate
            var existingEntry = allEntries.FirstOrDefault(e =>
                e.TimesheetId == timesheetId &&
                e.EntryDate.HasValue &&
                e.EntryDate.Value.Date == entryDate.Date
            );

            if (existingEntry != null)
            {
                return Json(new { exists = true, entryId = existingEntry.EntryId });
            }

            return Json(new { exists = false });
        }


    }
}
