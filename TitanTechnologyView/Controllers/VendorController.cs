using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TitanTechnologyView.Helpers;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class VendorController : Controller
    {        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiOrigin;
        private readonly string _apiUrl;

        public VendorController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiOrigin = apiSettings.Value.Origin;
            _apiUrl = $"{apiSettings.Value.BaseUrl}/Vendor";
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.GetAsync(_apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "API call failed: " + response.StatusCode;
                return View(new List<VendorDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var vendors = JsonConvert.DeserializeObject<List<VendorDto>>(json) ?? new();
            return View(vendors);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return Content("Vendor not found");

            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<CustomerFormDto>(json);

            // 2. Fetch vendor list
            //var vendors = await FetchListAsync<VendorDto>($"{_apiOrigin}/api/Vendor");

            //if (employee != null && employee.VendorId.HasValue)
            //{
            //    employee.VendorName = vendors.FirstOrDefault(x => x.VendorId == employee.VendorId.Value)?.VendorName ?? "N/A";
            //}

            return View("ViewCustomer", customer);
        }

        [HttpGet]
        public async Task<IActionResult> VendorForm(int? id = 0)
        {
            ViewBag.ApiOrigin = _apiOrigin;
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var companies = new List<CompanyMaster>();
            var companyResponse = await client.GetAsync($"{_apiOrigin}/api/Company");
            if (companyResponse.IsSuccessStatusCode)
            {
                var companyJson = await companyResponse.Content.ReadAsStringAsync();
                companies = JsonConvert.DeserializeObject<List<CompanyMaster>>(companyJson) ?? new();
            }
            ViewBag.Companies = companies;

            if (id == 0)
            {
                return View(new VendorDto());
            }

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<VendorDto>(json)?? new VendorDto();
            return View(model);
        }
       
        [HttpPost]
        public async Task<IActionResult> SaveVendor(VendorDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ApiOrigin = _apiOrigin;
                return View("VendorForm", model);
            }

            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            using var form = new MultipartFormDataContent();

            // Add all basic string fields
            form.Add(new StringContent(model.VendorId?.ToString() ?? "0"), "VendorId");
            form.Add(new StringContent(model.VendorName ?? ""), "VendorName");
            form.Add(new StringContent(model.CompanyCode ?? ""), "CompanyCode");
            form.Add(new StringContent(model.Address ?? ""), "Address");
            form.Add(new StringContent(model.Gstn ?? ""), "Gstn");
            form.Add(new StringContent(model.PanNumber ?? ""), "PanNumber");
            form.Add(new StringContent(model.ContactPersonName ?? ""), "ContactPersonName");
            form.Add(new StringContent(model.ContactPersonNumber ?? ""), "ContactPersonNumber");
            form.Add(new StringContent(model.PaymentTerms ?? ""), "PaymentTerms");
            form.Add(new StringContent(model.AccountHolderName ?? ""), "AccountHolderName"); 
            form.Add(new StringContent(model.AccountNumber1 ?? ""), "AccountNumber1"); 
            form.Add(new StringContent(model.IfscCode ?? ""), "IfscCode"); 
            form.Add(new StringContent(model.BankAccountName ?? ""), "BankAccountName");

            // Attach files via extension method
            await form.AttachFileAsync(client, "GstnUpload", model.GstnUpload, model.ExistingGstnUpload, _apiOrigin);
            await form.AttachFileAsync(client, "PanUpload", model.PanUpload, model.ExistingPanUpload, _apiOrigin);
            await form.AttachFileAsync(client, "CancelledCheque", model.CancelledCheque, model.ExistingCancelledCheque, _apiOrigin);
            await form.AttachFileAsync(client, "Agreement1", model.Agreement1, model.ExistingAgreement1, _apiOrigin);
            await form.AttachFileAsync(client, "Agreement2", model.Agreement2, model.ExistingAgreement2, _apiOrigin);
            await form.AttachFileAsync(client, "Agreement3", model.Agreement3, model.ExistingAgreement3, _apiOrigin);
            await form.AttachFileAsync(client, "Agreement4", model.Agreement4, model.ExistingAgreement4, _apiOrigin);

            var response = await client.PostAsync(_apiUrl, form);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            // on error, redisplay form
            ViewBag.ApiOrigin = _apiOrigin;
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("VendorForm", model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");
            var response = await client.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}


