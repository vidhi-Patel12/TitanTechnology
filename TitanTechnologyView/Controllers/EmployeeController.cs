using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TitanTechnologyView.Extensions;
using TitanTechnologyView.Helpers;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly string _apiOrigin;
        private readonly string _insertUpdateUrl;

        public EmployeeController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = $"{apiSettings.Value.BaseUrl}/Employee";
            _apiOrigin = apiSettings.Value.Origin;
            _insertUpdateUrl = $"{apiSettings.Value.BaseUrl}/Employee/InsertUpdate"; // for POST insert/update
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient("IgnoreSSL");

        private async Task<List<T>> FetchListAsync<T>(string url)
        {
            var client = CreateClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) 
            { 
                return new List<T>(); 
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }

        [HttpGet]
        public async Task<IActionResult> Index()                
        {
            var client = CreateClient();
            var response = await client.GetAsync(_apiUrl);

            // Employees
            var employees = await FetchListAsync<EmployeeFormDto>(_apiUrl);

            // Vendors
            var vendors = await FetchListAsync<VendorDto>($"{_apiOrigin}/api/Vendor");

            // 3. Map VendorName
            foreach (var emp in employees)
            {
                emp.VendorName = vendors.FirstOrDefault(v => v.VendorId == emp.VendorId)?.VendorName ?? "N/A";
            }
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> AddEmployee(int id = 0)
        {
            // 1. Fetch vendor list from API
            ViewBag.ApiOrigin = _apiOrigin;
            var client = CreateClient();

            ViewBag.EmployeeTypes = await client.GetDropdownAsync(_apiOrigin, "Employee Type");
            ViewBag.Companys = await FetchListAsync<CompanyMasterDto>($"{_apiOrigin}/api/Company");
            ViewBag.Vendors = await FetchListAsync<VendorDto>($"{_apiOrigin}/api/Vendor");
            ViewBag.TimingAvailabilitys = await client.GetDropdownAsync(_apiOrigin, "Timing Availability");
                       
            if (id == 0)
            {
                return View(new EmployeeFormDto());
            }

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<EmployeeFormDto>(json) ?? new EmployeeFormDto();

            // Map vendor name
            if (model.VendorId.HasValue)
            {
                var vendors = ViewBag.Vendors as List<VendorDto> ?? new List<VendorDto>();
                model.VendorName = vendors.FirstOrDefault(v => v.VendorId == model.VendorId.Value)?.VendorName;
            }

            // Selected company
            var companies = ViewBag.Companys as List<CompanyMasterDto> ?? new List<CompanyMasterDto>();
            var company = companies.FirstOrDefault(e => e.CompanyCode == model.CompanyCode);
            if (company != null)
            {
                ViewBag.SelectedCompanyName = company.CompanyName;
            }

            return View(model);
        }

          [HttpPost]
        public async Task<IActionResult> SaveEmployee(EmployeeFormDto model)
        {
            var client = CreateClient();
      
            if (!ModelState.IsValid)
            {
                ViewBag.Vendors = await FetchListAsync<VendorDto>($"{_apiOrigin}/api/Vendor");
                return View("AddEmployee", model);
            }

            using var content = new MultipartFormDataContent();
            // Base fields
            content.Add(new StringContent(model.EmployeeId.ToString()), "EmployeeId");
            content.Add(new StringContent(model.EmployeeType ?? ""), "EmployeeType");
            content.Add(new StringContent(model.CompanyCode ?? ""), "CompanyCode");
            if (model.VendorId.HasValue)
            {
                content.Add(new StringContent(model.VendorId.Value.ToString()), "VendorId");
            }
            content.Add(new StringContent(model.Name ?? ""), "Name");
            content.Add(new StringContent(model.AltName ?? ""), "AltName");
            content.Add(new StringContent(model.Age?.ToString() ?? ""), "Age");
            content.Add(new StringContent(model.SkillSet ?? ""), "SkillSet");
            content.Add(new StringContent(model.Experience?.ToString() ?? ""), "Experience");
            content.Add(new StringContent(model.TimingAvailability ?? ""), "TimingAvailability");
            content.Add(new StringContent(model.ContactNumber1 ?? ""), "ContactNumber1");
            content.Add(new StringContent(model.ContactNumber2 ?? ""), "ContactNumber2");
            content.Add(new StringContent(model.Remarks ?? ""), "Remarks");
            content.Add(new StringContent(model.ReferredBy ?? ""), "ReferredBy");
            content.Add(new StringContent(model.CreatedBy ?? ""), "CreatedBy");

            // Bank & PAN details
            content.Add(new StringContent(model.PanNumber ?? ""), "PanNumber1");
            content.Add(new StringContent(model.AccountNumber1 ?? ""), "AccountNumber1");
            content.Add(new StringContent(model.IfscCode1 ?? ""), "IfscCode1");
            content.Add(new StringContent(model.AccountName1 ?? ""), "AccountName1");
            content.Add(new StringContent(model.PanNumber2 ?? ""), "PanNumber2");
            content.Add(new StringContent(model.AccountNumber2 ?? ""), "AccountNumber2");
            content.Add(new StringContent(model.IfscCode2 ?? ""), "IfscCode2");
            content.Add(new StringContent(model.AccountName2 ?? ""), "AccountName2");

            // Files (using extension method)
            await content.AttachFileAsync(client, "NdaFile", model.NdaFile, model.NdaUpload, _apiOrigin);
            await content.AttachFileAsync(client, "AadharFile1", model.AadharFile1, model.AadharUpload, _apiOrigin);
            await content.AttachFileAsync(client, "PanFile1", model.PanFile1, model.PanUpload, _apiOrigin);
            await content.AttachFileAsync(client, "ChequeFile1", model.ChequeFile1, model.Cheque1Upload, _apiOrigin);
            await content.AttachFileAsync(client, "AadharFile2", model.AadharFile2, model.Aadhar2Upload, _apiOrigin);
            await content.AttachFileAsync(client, "PanFile2", model.PanFile2, model.PanUpload2, _apiOrigin);
            await content.AttachFileAsync(client, "ChequeFile2", model.ChequeFile2, model.Cheque2Upload, _apiOrigin);

            // POST for insert/update
            var response = await client.PostAsync(_insertUpdateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ApiOrigin = _apiOrigin;
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("AddEmployee", model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return Content("Employee not found");

            var json = await response.Content.ReadAsStringAsync();
            var employee = JsonConvert.DeserializeObject<EmployeeFormDto>(json);

            // 2. Fetch vendor list
            var vendors = await FetchListAsync<VendorDto>($"{_apiOrigin}/api/Vendor");
            
            if (employee != null && employee.VendorId.HasValue) 
            {
                employee.VendorName = vendors.FirstOrDefault(x => x.VendorId == employee.VendorId.Value)?.VendorName ?? "N/A";    
            }

            return View("ViewEmployee", employee);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();
            await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
