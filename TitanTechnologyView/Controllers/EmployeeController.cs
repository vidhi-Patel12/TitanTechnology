using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _apiOrigin = "https://localhost:44368";
        private readonly string _apiUrl = "https://localhost:44368/api/Employee"; // for GET/DELETE
        private readonly string _insertUpdateUrl = "https://localhost:44368/api/Employee/InsertUpdate"; // for POST insert/update

        public EmployeeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()                
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(_apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "API call failed: " + response.StatusCode;
                return View(new List<EmployeeFormDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var employees = JsonConvert.DeserializeObject<List<EmployeeFormDto>>(json) ?? new();

            var vendorResponse = await client.GetAsync($"{_apiOrigin}/api/Vendor");
            var vendors = new List<VendorDto>();
            if (vendorResponse.IsSuccessStatusCode)
            {
                var vendorJson = await vendorResponse.Content.ReadAsStringAsync();
                vendors = JsonConvert.DeserializeObject<List<VendorDto>>(vendorJson) ?? new();
            }

            // 3. Map VendorName
            foreach (var emp in employees)
            {
                var vendor = vendors.FirstOrDefault(v => v.VendorId == emp.VendorId);
                emp.VendorName = vendor?.VendorName ?? "N/A";
            }
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> AddEmployee(int id = 0)
        {
            // 1. Fetch vendor list from API
            ViewBag.ApiOrigin = _apiOrigin;
            var client = _httpClientFactory.CreateClient();

            var companyResponse = await client.GetAsync($"{_apiOrigin}/api/Company");
            var companys = new List<CompanyMasterDto>();

            if (companyResponse.IsSuccessStatusCode)
            {
                var companyJson = await companyResponse.Content.ReadAsStringAsync();
                companys = JsonConvert.DeserializeObject<List<CompanyMasterDto>>(companyJson) ?? new List<CompanyMasterDto>();
            }
            ViewBag.Companys = companys;


            var vendorResponse = await client.GetAsync($"{_apiOrigin}/api/Vendor");
            var vendors = new List<VendorDto>();
            if (vendorResponse.IsSuccessStatusCode)
            {
                var vendorJson = await vendorResponse.Content.ReadAsStringAsync();
                vendors = JsonConvert.DeserializeObject<List<VendorDto>>(vendorJson) ?? new List<VendorDto>();
            }
            ViewBag.Vendors = vendors;

            if (id == 0)
            {
                return View(new EmployeeFormDto());
            }

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<EmployeeFormDto>(json) ?? new EmployeeFormDto();

            if (model.VendorId.HasValue)
            {
                var vendor = vendors.FirstOrDefault(v => v.VendorId == model.VendorId.Value);
                if (vendor != null)
                    model.VendorName = vendor.VendorName;
            }

            var company = companys.FirstOrDefault(e => e.CompanyCode == model.CompanyCode);

            if (company != null)
            {
                ViewBag.SelectedCompanyName = company.CompanyName;
            }
            return View(model);
        }

        // Helper: guess mime type by extension
        private static string GuessMime(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmployee(EmployeeFormDto model)
        {
            var client = _httpClientFactory.CreateClient();
      
            if (!ModelState.IsValid)
            {
                var vendorResponse = await client.GetAsync($"{_apiOrigin}/api/Vendor");
                if (vendorResponse.IsSuccessStatusCode)
                {
                    var vendorJson = await vendorResponse.Content.ReadAsStringAsync();
                    ViewBag.Vendors = JsonConvert.DeserializeObject<List<VendorDto>>(vendorJson);
                }
                return View("AddEmployee", model);
            }

            using var content = new MultipartFormDataContent();

            // Base fields
            content.Add(new StringContent(model.EmployeeId.ToString()), "EmployeeId");
            content.Add(new StringContent(model.EmployeeType ?? ""), "EmployeeType");
            content.Add(new StringContent(model.CompanyCode ?? ""), "CompanyCode");
            if (model.VendorId.HasValue)
                content.Add(new StringContent(model.VendorId.Value.ToString()), "VendorId");

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
            content.Add(new StringContent(model.PanNumber ?? ""), "PanNumber1");
            content.Add(new StringContent(model.AccountNumber1 ?? ""), "AccountNumber1");
            content.Add(new StringContent(model.IfscCode1 ?? ""), "IfscCode1");
            content.Add(new StringContent(model.AccountName1 ?? ""), "AccountName1");
            content.Add(new StringContent(model.PanNumber2 ?? ""), "PanNumber2");
            content.Add(new StringContent(model.AccountNumber2 ?? ""), "AccountNumber2");
            content.Add(new StringContent(model.IfscCode2 ?? ""), "IfscCode2");
            content.Add(new StringContent(model.AccountName2 ?? ""), "AccountName2");

            // File attachments
            // local helper for attaching new OR existing files
            async Task AttachAsync(string field, IFormFile? newFile, string? existingRelativeUrl)
            {
                if (newFile != null)
                {
                    var sc = new StreamContent(newFile.OpenReadStream());
                    sc.Headers.ContentType = new MediaTypeHeaderValue(newFile.ContentType);
                    content.Add(sc, field, newFile.FileName);
                }

                // If no new file uploaded but old path exists → fetch and reattach
                if (!string.IsNullOrWhiteSpace(existingRelativeUrl))
                {
                    var absoluteUrl = existingRelativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? existingRelativeUrl
                        : new Uri(new Uri(_apiOrigin), existingRelativeUrl).ToString(); // Use Uri to combine properly

                    var fileBytes = await client.GetByteArrayAsync(absoluteUrl);
                    var fileName = Path.GetFileName(existingRelativeUrl);
                    var ba = new ByteArrayContent(fileBytes);
                    ba.Headers.ContentType = new MediaTypeHeaderValue(GuessMime(fileName));
                    content.Add(ba, field, fileName);
                }

                // else nothing → API will clear it
            }

            await AttachAsync("NdaFile", model.NdaFile, model.NdaUpload);
            await AttachAsync("AadharFile1", model.AadharFile1, model.AadharUpload);
            await AttachAsync("PanFile1", model.PanFile1, model.PanUpload);
            await AttachAsync("ChequeFile1", model.ChequeFile1, model.Cheque1Upload);
            await AttachAsync("AadharFile2", model.AadharFile2, model.Aadhar2Upload);
            await AttachAsync("PanFile2", model.PanFile2, model.PanUpload2);
            await AttachAsync("ChequeFile2", model.ChequeFile2, model.Cheque2Upload);

            // POST for insert/update
            var response = await client.PostAsync(_insertUpdateUrl, content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ViewBag.ApiOrigin = _apiOrigin;
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("AddEmployee", model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
