using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TitanTechnologyView.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TitanTechnologyView.Controllers
{
    public class TimesheetController : Controller
    {
        private readonly string _apiOrigin = "https://api.titentechnology.com";
        private readonly string apiBaseUrl = "https://api.titentechnology.com/api/Timesheet";
        private readonly IHttpClientFactory _httpClientFactory;

        public TimesheetController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
        [HttpGet]
        public async Task<IActionResult> LoadByMonth(int year, int month)
        {
            var client = CreateClients();
            var loginId = HttpContext.Request.Cookies["LoginId"]; // logged-in employee

            // Only fetch timesheets for the logged-in employee
            var response = await client.GetAsync($"{_apiOrigin}/api/Timesheet/ByMonth?year={year}&month={month}&employeeId={loginId}");

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "API call failed" });

            var data = await response.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();

            return Json(new { success = true, data }); // data already filtered
        }

        // Show Add/Edit Form
        public async Task<IActionResult> AddEditAsync(int? id)
        {
            ViewBag.ApiOrigin = _apiOrigin;
            var client = CreateClients();

            // Fetch employees
            var employees = new List<EmployeeFormDto>();
            var employeeResponse = await client.GetAsync($"{_apiOrigin}/api/Employee");

            if (employeeResponse.IsSuccessStatusCode)
            {
                employees = await employeeResponse.Content.ReadFromJsonAsync<List<EmployeeFormDto>>() ?? new();
            }

            var employeeDisplayList = employees.Select(e => new
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name
            }).ToList();

            ViewBag.Employees = employeeDisplayList;

            // Fetch timesheets
            List<Timesheet> timesheets = new List<Timesheet>();

            if (id.HasValue && id > 0)
            {
                // Single timesheet by ID
                var response = await client.GetAsync($"{_apiOrigin}/api/Timesheet/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var ts = await response.Content.ReadFromJsonAsync<Timesheet>();
                    if (ts != null) timesheets.Add(ts);
                }
            }
            else
            {
                // Fetch all timesheets for the current employee (or all employees)
                var loginId = HttpContext.Request.Cookies["LoginId"];
                var response = await client.GetAsync($"{_apiOrigin}/api/Timesheet/ByEmployee?employeeId={loginId}");
                if (response.IsSuccessStatusCode)
                {
                    timesheets = await response.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new List<Timesheet>();
                }
            }

            return View(timesheets);  // Pass List<Timesheet> to view
        }

        // Handle Save (Add or Update)
        //[HttpPost]
        //public async Task<IActionResult> SaveTimesheets([FromBody] List<Timesheet> timesheets)
        //{
        //    if (timesheets == null || !timesheets.Any())
        //        return BadRequest("No timesheets to save.");

        //    var client = CreateClients(); // only once

        //    string jsonData = JsonConvert.SerializeObject(timesheets);
        //    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        //    var response = await client.PostAsync($"{_apiOrigin}/api/Timesheet/InsertUpdate", content);

        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode(500, "Error saving timesheets");

        //    var resultJson = await response.Content.ReadAsStringAsync();
        //    var savedTimesheets = JsonConvert.DeserializeObject<List<Timesheet>>(resultJson);

        //    return Ok(savedTimesheets);
        //}

        [HttpPost]
        public async Task<IActionResult> SaveTimesheets([FromBody] List<Timesheet> timesheets)
        {
            if (timesheets == null || !timesheets.Any())
                return BadRequest("No timesheets to save.");

            var client = CreateClients(); // HTTP client for API calls

            // Step 1: Check each timesheet individually via GET API
            foreach (var ts in timesheets)
            {
                if (ts.TimesheetId > 0)
                    continue;


                var checkResponse = await client.GetAsync(
                    $"{_apiOrigin}/api/Timesheet/GetByProjectCodeWorkMonth?projectcode={ts.ProjectCode}&workmonth={ts.WorkMonth:yyyy-MM-dd}&employeeid={ts.EmployeeId}"
                );

                if (checkResponse.IsSuccessStatusCode)
                {
                    var existingJson = await checkResponse.Content.ReadAsStringAsync();
                    var existingTimesheet = JsonConvert.DeserializeObject<List<Timesheet>>(existingJson);

                    if (existingTimesheet != null && existingTimesheet.Any())
                    {
                        var existing = existingTimesheet.First();
                        return BadRequest(
                             $"Timesheet is already exists."
                        //$"Timesheet for Project '{ts.ProjectCode}' and WorkMonth '{ts.WorkMonth:yyyy-MM-dd}' and Employee '{ts.EmployeeId}' already exists."
                        );
                    }

                }
                else if (checkResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    // If API fails for reasons other than NotFound, return error
                    return StatusCode(500, $"Error checking timesheet for Project '{ts.ProjectCode}' and WorkMonth '{ts.WorkMonth:yyyy-MM-dd}' and Employee '{ts.EmployeeId}'.");
                }
            }

            // Step 2: Call Insert/Update API if none exist
            string jsonData = JsonConvert.SerializeObject(timesheets);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiOrigin}/api/Timesheet/InsertUpdate", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode(500, "saving timesheets");

            var resultJson = await response.Content.ReadAsStringAsync();
            var savedTimesheets = JsonConvert.DeserializeObject<List<Timesheet>>(resultJson);

            return Ok(savedTimesheets);
        }


        // Show List Page
        public async Task<IActionResult> Index()
        {
            var client = CreateClients();

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
            var client = CreateClients();

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
            var client = CreateClients();

            var allTimesheetsResponse = await client.GetAsync(apiBaseUrl);
            if (allTimesheetsResponse.IsSuccessStatusCode)
            {
                var allTimesheets = await allTimesheetsResponse.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();

                bool exists = allTimesheets.Any(ts =>
                    ts.ProjectCode == projectCode &&
                    ts.EmployeeId == employeeId &&
                    ts.TimesheetType == timesheetType &&
                    //ts.MonthYear == monthYear &&
                    ts.TimesheetId != timesheetId);

                if (exists)
                {
                    return Json(new { isDuplicate = true });
                }
            }

            return Json(new { isDuplicate = false });
        }

        [HttpGet("exportview")]
        public IActionResult ExportTimesheetReportView()
        {
            return View("ExportTimesheetReport");
        }

        [HttpGet("timesheetreport/export")]
        public async Task<IActionResult> ExportTimesheetReport(DateTime? workMonth = null, string timesheetType = null)
        {
            // 1. Create HttpClient (you can use IHttpClientFactory in real projects)
            using var client = CreateClients();

            // _apiOrigin = "https://localhost:44368/api"

            // Add optional query parameters only if provided
            //var query = $"{_apiOrigin}/Timesheet/GetTimesheetReport";
            var query = $"{_apiOrigin}/api/Timesheet/GetTimesheetReport";

            var queryParams = new List<string>();

            if (workMonth.HasValue)
                queryParams.Add($"workMonth={workMonth.Value:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(timesheetType))
                queryParams.Add($"timesheetType={timesheetType}");

            query += queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;


            // Call the API
            var response = await client.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API ERROR ({(int)response.StatusCode}): {errorText}");
                TempData["ErrorMessage"] = $"Failed to fetch timesheet data. API returned {(int)response.StatusCode}.";
                return RedirectToAction(nameof(ExportTimesheetReportView));
            }


            // Deserialize JSON response
            var timesheets = await response.Content.ReadFromJsonAsync<List<Timesheet>>() ?? new();

            // 3. Generate Excel file
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TimesheetReport");

            // 4. Add headers (same as your existing code)
            // 1️ Header info section
            worksheet.Cell(1, 1).Value = "Mail Id";
            worksheet.Cell(1, 2).Value = timesheets.FirstOrDefault()?.EmployeeEmail ?? "";

            worksheet.Cell(2, 1).Value = "Work Month";
            worksheet.Cell(2, 2).Value = timesheets.FirstOrDefault()?.WorkMonth?.ToString("yyyy-MM") ?? "";

            worksheet.Cell(3, 1).Value = "Employee Number";
            worksheet.Cell(3, 2).Value = timesheets.FirstOrDefault()?.EmployeeId.ToString() ?? "";

            worksheet.Cell(4, 1).Value = "Name";
            worksheet.Cell(4, 2).Value = timesheets.FirstOrDefault()?.EmployeeName ?? "";

            // Make header labels bold
            for (int i = 1; i <= 4; i++)
            {
                worksheet.Cell(i, 1).Style.Font.Bold = true;
            }

            // Add spacing before table
            worksheet.Row(5).Height = 10;

            // 2️⃣ Table headers
            worksheet.Cell(6, 1).Value = "Project Code";
            worksheet.Cell(6, 2).Value = "Item Name";
            worksheet.Cell(6, 3).Value = "Timesheet Type";
            worksheet.Cell(6, 4).Value = "Rate";
            worksheet.Cell(6, 5).Value = "Unit";
            worksheet.Cell(6, 6).Value = "Monthly Work Days";
            worksheet.Cell(6, 7).Value = "Holiday";
            worksheet.Cell(6, 8).Value = "Leave";
            worksheet.Cell(6, 9).Value = "Extra Days";
            worksheet.Cell(6, 10).Value = "Actual Work Days/Hours";
            worksheet.Cell(6, 11).Value = "Upload Timesheet";
            worksheet.Cell(6, 12).Value = "Approve / Reject";
            worksheet.Cell(6, 13).Value = "Approved Working Days";
            worksheet.Cell(6, 14).Value = "Net Rate";
            worksheet.Cell(6, 15).Value = "TDS Applicable";
            worksheet.Cell(6, 16).Value = "Net Payable";
            worksheet.Cell(6, 17).Value = "Net Payable After TDS";
            worksheet.Cell(6, 18).Value = "Paid";
            //worksheet.Cell(6, 19).Value = "Paid Reference Number";

            // Make header row bold and add borders
            //var headerRange = worksheet.Range("A6:S6");
            //headerRange.Style.Font.Bold = true;
            //headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            //headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 3️ Table data
            int currentRow = 7;
            foreach (var ts in timesheets)
            {
                worksheet.Cell(currentRow, 1).Value = ts.ProjectCode ?? "";
                worksheet.Cell(currentRow, 2).Value = ts.ProjectName ?? "";
                worksheet.Cell(currentRow, 3).Value = ts.TimesheetType ?? "";
                worksheet.Cell(currentRow, 4).Value = ts.rate.ToString() ?? "";
                worksheet.Cell(currentRow, 5).Value = ts.unit ?? "";
                worksheet.Cell(currentRow, 6).Value = ts.monthlyworkunit.ToString() ?? "";
                worksheet.Cell(currentRow, 7).Value = ts.holiday.ToString() ?? "";
                worksheet.Cell(currentRow, 8).Value = ts.leave.ToString() ?? "";
                worksheet.Cell(currentRow, 9).Value = ts.extradays.ToString() ?? "";
                worksheet.Cell(currentRow, 10).Value = ts.actualworkdayshours.ToString() ?? "";
                worksheet.Cell(currentRow, 11).Value = ts.uploadtimesheet ?? "";
                worksheet.Cell(currentRow, 12).Value = ts.status ?? "";
                worksheet.Cell(currentRow, 13).Value = ts.approvedworkingunit.ToString() ?? "";
                worksheet.Cell(currentRow, 14).Value = ts.netrate.ToString() ?? "";
                worksheet.Cell(currentRow, 15).Value = ts.tdsapplicable.ToString() ?? "";
                worksheet.Cell(currentRow, 16).Value = ts.netpayble.ToString() ?? "";
                worksheet.Cell(currentRow, 17).Value = ts.netpaybleaftertds.ToString() ?? "";
                worksheet.Cell(currentRow, 18).Value = ts.salarypaid ?? "";
                //worksheet.Cell(currentRow, 19).Value = ts.salarypaid ?? "";

                currentRow++;
            }

            // Add borders to data
            //var dataRange = worksheet.Range($"A6:S{currentRow - 1}");
            //dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Auto adjust column width
            worksheet.Columns().AdjustToContents();

            // 6. Return Excel file
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"TimesheetReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


    }
}
