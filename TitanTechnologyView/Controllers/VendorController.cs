using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class VendorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiOrigin = "https://localhost:44368";            
        private readonly string _apiUrl = "https://localhost:44368/api/Vendor";

        public VendorController(IHttpClientFactory httpClientFactory)
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
                return View(new List<VendorDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var vendors = JsonConvert.DeserializeObject<List<VendorDto>>(json) ?? new();
            return View(vendors);
        }

        [HttpGet]
        public async Task<IActionResult> VendorForm(int? id = 0)
        {
            ViewBag.ApiOrigin = _apiOrigin;
            var client = _httpClientFactory.CreateClient();

            var companyResponse = await client.GetAsync($"{_apiOrigin}/api/Company");
            var companies = new List<CompanyMaster>();
            if (companyResponse.IsSuccessStatusCode)
            {
                var companyJson = await companyResponse.Content.ReadAsStringAsync();
                companies = JsonConvert.DeserializeObject<List<CompanyMaster>>(companyJson) ?? new();
            }
            ViewBag.Companies = companies;


            if (id == 0)
                return View(new VendorDto());

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<VendorDto>(json)?? new VendorDto();
            return View(model);
        }

        // Helper: guess mime type by file extension
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
        public async Task<IActionResult> SaveVendor(VendorDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ApiOrigin = _apiOrigin;
                return View("VendorForm", model);
            }

            var client = _httpClientFactory.CreateClient();
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

            // Attach files
            async Task AttachFile(string field, IFormFile? newFile, string? existingRelativeUrl)
            {
                if (newFile != null)
                {
                    var sc = new StreamContent(newFile.OpenReadStream());
                    sc.Headers.ContentType = new MediaTypeHeaderValue(newFile.ContentType);
                    form.Add(sc, field, newFile.FileName);
                    return;
                }

                // No new file: if we have an existing path like "/agreements/abc.pdf", fetch it from API and re-attach
                if (!string.IsNullOrWhiteSpace(existingRelativeUrl))
                {
                    var absoluteUrl = existingRelativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? existingRelativeUrl
                        : $"{_apiOrigin}{existingRelativeUrl}";

                    var fileBytes = await client.GetByteArrayAsync(absoluteUrl);
                    var fileName = Path.GetFileName(existingRelativeUrl);
                    var ba = new ByteArrayContent(fileBytes);
                    ba.Headers.ContentType = new MediaTypeHeaderValue(GuessMime(fileName));
                    form.Add(ba, field, fileName);
                }
                // else: nothing attached -> API will null it
            }

            await AttachFile("GstnUpload", model.GstnUpload, model.ExistingGstnUpload);
            await AttachFile("PanUpload", model.PanUpload, model.ExistingPanUpload);
            await AttachFile("CancelledCheque", model.CancelledCheque, model.ExistingCancelledCheque);
            await AttachFile("Agreement1", model.Agreement1, model.ExistingAgreement1);
            await AttachFile("Agreement2", model.Agreement2, model.ExistingAgreement2);
            await AttachFile("Agreement3", model.Agreement3, model.ExistingAgreement3);
            await AttachFile("Agreement4", model.Agreement4, model.ExistingAgreement4);

            var response = await client.PostAsync(_apiUrl, form);
 
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            // on error, redisplay form
            ViewBag.ApiOrigin = _apiOrigin;
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("VendorForm", model);
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


