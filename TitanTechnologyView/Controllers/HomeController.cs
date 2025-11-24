using internalPortalFroent.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using TitanTechnologyView.Models;
using static System.Net.WebRequestMethods;

namespace internalPortalFroent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _apiBase = apiSettings.Value.BaseUrl;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            var client = CreateClients();
            var response = await client.PostAsJsonAsync($"{_apiBase}/Contact/SaveMessage", model);

            string apiResponse = await response.Content.ReadAsStringAsync();

            // If API returned error → pass through
            if (!response.IsSuccessStatusCode)
                return Content(apiResponse, "application/json");

            // Convert JSON string → JSON object
            return Content(apiResponse, "application/json");
        }




        public IActionResult Careers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitApplication([FromForm] JobApplication model, IFormFile ResumeFile)
        {
            var response = await SubmitApplicationAsync(model, ResumeFile);
            return Ok(response);
        }


        public async Task<object> SubmitApplicationAsync(JobApplication model, IFormFile ResumeFile)
        {
            var client = CreateClients();

            var form = new MultipartFormDataContent();

            // If resume file exists
            if (ResumeFile != null && ResumeFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ResumeFile.FileName);

                // ADD PDF FILE
                var stream = ResumeFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                form.Add(fileContent, "ResumeFile", fileName);

                // SEND SAME NAME IN ResumeUrl FIELD ALSO
                form.Add(new StringContent("/resumes/" + fileName), "ResumeUrl");

                // Also update model
                model.ResumeUrl = "/resumes/" + fileName;
            }
            else
            {
                form.Add(new StringContent(""), "ResumeUrl");
            }

            // ADD OTHER FIELDS
            form.Add(new StringContent(model.CareerId.ToString()), "CareerId");
            form.Add(new StringContent(model.ApplicantName), "ApplicantName");
            form.Add(new StringContent(model.Email), "Email");
            form.Add(new StringContent(model.Phone ?? ""), "Phone");
            form.Add(new StringContent(string.IsNullOrWhiteSpace(model.CoverLetter) ? "" : model.CoverLetter), "CoverLetter");

            // SEND REQUEST TO API
            var response = await client.PostAsync($"{_apiBase}/JobApplication/Apply", form);
            return await response.Content.ReadAsStringAsync();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult ServiceDetails(int id)
        {
            return View();
        }

        [HttpGet("/Service/Get/{id}")]
        public async Task<IActionResult> GetService(int id)
        {
            var client = CreateClients();
            var response = await client.GetAsync($"{_apiBase}/Service/{id}"); //include /api

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("/Service/Image")]
        public async Task<IActionResult> GetServiceImage(string path)
        {
            var client = CreateClients(); // uses IgnoreSSL, bypasses invalid certs
            var imageUrl = $"https://api.titentechnology.com/{path?.TrimStart('/')}";
            var response = await client.GetAsync(imageUrl);
            if (response.IsSuccessStatusCode)
                if (!response.IsSuccessStatusCode)
                    return NotFound();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

            return File(bytes, contentType);
        }


        public IActionResult Solutions()
        {
            return View();
        }

        public IActionResult SolutionDetails(int id)
        {
            return View();
        }

        [HttpGet("/Solution/Get/{id}")]
        public async Task<IActionResult> GetSolution(int id)
        {
            var client = CreateClients();
            var response = await client.GetAsync($"{_apiBase}/Solution/{id}"); //include /api

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("/Solution/Image")]
        public async Task<IActionResult> GetSolutionImage(string path)
        {
            var client = CreateClients(); // uses IgnoreSSL, bypasses invalid certs
            //var imageUrl = $"https://api.titentechnology.com{path}";
            var imageUrl = $"https://api.titentechnology.com/{path?.TrimStart('/')}";

            var response = await client.GetAsync(imageUrl);
            if (response.IsSuccessStatusCode)
                if (!response.IsSuccessStatusCode)
                    return NotFound();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

            return File(bytes, contentType);
        }

        public PartialViewResult Header()
        {
            return PartialView("_Header");
        }

        public PartialViewResult Footer()
        {
            return PartialView("_Footer");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            // Attach InternalPortalAuth cookie if exists
            var cookie = HttpContext.Request.Cookies["InternalPortalAuth"];
            if (!string.IsNullOrEmpty(cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", $"InternalPortalAuth={cookie}");
            }

            var response = await client.GetAsync("https://api.titentechnology.com/api/Service");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetSolutions()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            var cookie = HttpContext.Request.Cookies["InternalPortalAuth"];
            if (!string.IsNullOrEmpty(cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", $"InternalPortalAuth={cookie}");
            }

            var response = await client.GetAsync("https://api.titentechnology.com/api/Solution");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}
