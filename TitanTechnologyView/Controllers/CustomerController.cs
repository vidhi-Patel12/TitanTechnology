// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TitanTechnologyView.Helpers;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly string _apiOrigin;

        public CustomerController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = $"{apiSettings.Value.BaseUrl}/Customer";
            _apiOrigin = apiSettings.Value.Origin;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(_apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "API call failed: " + response.StatusCode;
                return View(new List<CustomerMaster>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<List<CustomerMaster>>(json) ?? new();

            var companyResponse = await client.GetAsync($"{_apiOrigin}/api/Company");
            var companies = new List<CompanyMasterDto>();
            if (companyResponse.IsSuccessStatusCode)
            {
                var companyJson = await companyResponse.Content.ReadAsStringAsync();
                companies = JsonConvert.DeserializeObject<List<CompanyMasterDto>>(companyJson) ?? new();
            }

            // 3. Map CompanyName into each customer
            foreach (var customer in customers)
            {
                customer.CompanyName = companies.FirstOrDefault(c => c.CompanyCode == customer.CompanyCode)?.CompanyName;
            }

            return View(customers);
        }

        [HttpGet]
        public async Task<IActionResult> AddCustomer(int id = 0)
        {
            ViewBag.ApiOrigin = _apiOrigin; // so the view can build absolute links
            var client = _httpClientFactory.CreateClient();

            var companyResponse = await client.GetAsync($"{_apiOrigin}/api/Company");
            var companies = new List<CompanyMasterDto>();

            if (companyResponse.IsSuccessStatusCode)
            {
                var companyJson = await companyResponse.Content.ReadAsStringAsync();
                companies = JsonConvert.DeserializeObject<List<CompanyMasterDto>>(companyJson) ?? new List<CompanyMasterDto>();
            }
            ViewBag.Companys = companies;

            if (id == 0)
            {
                return View(new CustomerFormDto());
            }

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CustomerFormDto>(json) ?? new CustomerFormDto();

            ViewBag.SelectedCompanyName = companies.FirstOrDefault(e => e.CompanyCode == model.CompanyCode)?.CompanyName;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomer(CustomerFormDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ApiOrigin = _apiOrigin;
                return View("AddCustomer", model);
            }

            var client = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();

            // Base fields
            content.Add(new StringContent(model.CustomerId.ToString()), "CustomerId");
            content.Add(new StringContent(model.CompanyCode ?? ""), "CompanyCode");
            content.Add(new StringContent(model.CustomerName ?? ""), "CustomerName");
            content.Add(new StringContent(model.Address ?? ""), "Address");
            content.Add(new StringContent(model.Country ?? ""), "Country");
            content.Add(new StringContent(model.Gstn ?? ""), "Gstn");
            content.Add(new StringContent(model.PanNumber ?? ""), "PanNumber");
            content.Add(new StringContent(model.ContactPersonName ?? ""), "ContactPersonName");
            content.Add(new StringContent(model.ContactPersonNumber ?? ""), "ContactPersonNumber");
            content.Add(new StringContent(model.PaymentTerms ?? ""), "PaymentTerms");

            // local function to attach new-or-existing file    
            await content.AttachFileAsync(client, "AgreementFile1", model.AgreementFile1, model.Agreement1, _apiOrigin);
            await content.AttachFileAsync(client, "AgreementFile2", model.AgreementFile2, model.Agreement2, _apiOrigin);
            await content.AttachFileAsync(client, "AgreementFile3", model.AgreementFile3, model.Agreement3, _apiOrigin);
            await content.AttachFileAsync(client, "AgreementFile4", model.AgreementFile4, model.Agreement4, _apiOrigin);


            // Your API uses POST for both insert/update
            var response = await client.PostAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            // on error, redisplay form
            ViewBag.ApiOrigin = _apiOrigin;
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("AddCustomer", model);
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
