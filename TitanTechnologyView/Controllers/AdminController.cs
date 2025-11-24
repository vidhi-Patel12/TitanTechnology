using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase;
        public AdminController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            //_apiBase = apiSettings.Value.BaseUrl;
            _apiBase = $"{apiSettings.Value.BaseUrl}";
        }


        private HttpClient CreateClients()
        {
            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            // Attach token from cookie (server side)
            if (Request.Cookies.TryGetValue("AuthToken", out var token) && !string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            return client;
        }


        public IActionResult Dropdown()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDropdowns()
        {
            var client = CreateClients();
            var response = await client.GetAsync($"{_apiBase}/DropdownMaster");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpPost("/Admin/SaveDropdown")]
        public async Task<IActionResult> SaveDropdown([FromBody] DropdownMaster model)
        {
            var client = CreateClients();

            //  Read token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBase}/DropdownMaster", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API returned {response.StatusCode}: {result}");
                return StatusCode((int)response.StatusCode, result);
            }

            return Content(result, "application/json");
        }


        [HttpDelete("/Admin/DeleteDropdown/{id}")]
        public async Task<IActionResult> DeleteDropdown(int id, int updatedBy)
        {
            var client = CreateClients();

            // Attach JWT token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Call backend API
            var response = await client.DeleteAsync($"{_apiBase}/DropdownMaster/{id}?updatedBy={updatedBy}");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API returned {response.StatusCode}: {result}");
                return StatusCode((int)response.StatusCode, result);
            }

            return Content(result, "application/json");
        }


        public IActionResult Service()
        {
            return View();
        }

        [HttpGet("/Admin/GetServices")]
        public async Task<IActionResult> GetServices()
        {
            var client = CreateClients(); // your HttpClientFactory("IgnoreSSL")
            var response = await client.GetAsync($"{_apiBase}/Service");

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("/Admin/GetServices/{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var client = CreateClients();

                // Add token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Call backend API
                var response = await client.GetAsync($"{_apiBase}/Service/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error fetching service {id}: {response.StatusCode} - {json}");
                    return StatusCode((int)response.StatusCode, json);
                }

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving service: {ex.Message}");
            }
        }


        [HttpPost("/Admin/SaveService")]
        public async Task<IActionResult> SaveService(IFormCollection form)
        {
            try
            {
                var client = CreateClients();

                //  Read JWT token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                //  Prepare form data for backend API
                using var multipart = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                {
                    multipart.Add(new StringContent(form[key]), key);
                }

                //  Handle file upload
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    multipart.Add(streamContent, "imageFile", file.FileName);
                }

                // Forward to backend API (adjust endpoint name)
                var response = await client.PostAsync($"{_apiBase}/Service/Post", multipart);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        //  4. Update Service
        [HttpPut("/Admin/UpdateService")]
        public async Task<IActionResult> UpdateService(IFormCollection form)
        {
            try
            {
                var client = CreateClients();

                //  Read JWT token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                //  Prepare multipart form data for backend API
                using var multipart = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                {
                    multipart.Add(new StringContent(form[key]), key);
                }

                //  Include image file if provided
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    multipart.Add(streamContent, "imageFile", file.FileName);
                }

                //  Forward to backend API endpoint (PUT)
                var response = await client.PutAsync($"{_apiBase}/Service/Update", multipart);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        //  5. Delete Service
        [HttpDelete("/Admin/DeleteService/{id}")]
        public async Task<IActionResult> DeleteService(int id, [FromQuery] int updatedBy)
        {
            var client = CreateClients();
            var response = await client.DeleteAsync($"{_apiBase}/Service/{id}?updatedBy={updatedBy}");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return BadRequest(result);

            return Content(result, "application/json");
        }

        public IActionResult Solution()
        {
            return View();
        }

        [HttpGet("/Admin/GetSolutions")]
        public async Task<IActionResult> GetSolutions()
        {
            var client = CreateClients(); 
            var response = await client.GetAsync($"{_apiBase}/Solution");

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("/Admin/GetSolutions/{id}")]
        public async Task<IActionResult> GetSolutionsById(int id)
        {
            try
            {
                var client = CreateClients();

                // Add token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Call backend API
                var response = await client.GetAsync($"{_apiBase}/Solution/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error fetching solution {id}: {response.StatusCode} - {json}");
                    return StatusCode((int)response.StatusCode, json);
                }

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving solution: {ex.Message}");
            }
        }



        [HttpPost("/Admin/SaveSolution")]
        public async Task<IActionResult> SaveSolution(IFormCollection form)
        {
            try
            {
                var client = CreateClients();

                //  Read JWT token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                //  Prepare form data for backend API
                using var multipart = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                {
                    multipart.Add(new StringContent(form[key]), key);
                }

                //  Handle file upload
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    multipart.Add(streamContent, "imageFile", file.FileName);
                }

                // Forward to backend API (adjust endpoint name)
                var response = await client.PostAsync($"{_apiBase}/Solution/Post", multipart);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        //  4. Update Service
        [HttpPut("/Admin/UpdateSolution")]
        public async Task<IActionResult> UpdateSolution(IFormCollection form)
        {
            try
            {
                var client = CreateClients();

                //  Read JWT token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                //  Prepare multipart form data for backend API
                using var multipart = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                {
                    multipart.Add(new StringContent(form[key]), key);
                }

                //  Include image file if provided
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    multipart.Add(streamContent, "imageFile", file.FileName);
                }

                //  Forward to backend API endpoint (PUT)
                var response = await client.PutAsync($"{_apiBase}/Solution/Update", multipart);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        //  5. Delete Service
        [HttpDelete("/Admin/DeleteSolution/{id}")]
        public async Task<IActionResult> DeleteSolution(int id, [FromQuery] int updatedBy)
        {
            var client = CreateClients();
            var response = await client.DeleteAsync($"{_apiBase}/Solution/{id}?updatedBy={updatedBy}");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return BadRequest(result);

            return Content(result, "application/json");
        }


        public IActionResult Career()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCareers()
        {
            var client = CreateClients();
            var response = await client.GetAsync($"{_apiBase}/Career");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("/Admin/GetCareers/{id}")]
        public async Task<IActionResult> GetCareerById(int id)
        {
            try
            {
                var client = CreateClients();

                // Add token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Call backend API
                var response = await client.GetAsync($"{_apiBase}/Career/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error fetching career {id}: {response.StatusCode} - {json}");
                    return StatusCode((int)response.StatusCode, json);
                }

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving career: {ex.Message}");
            }
        }

        [HttpPost("/Admin/SaveCareer")]
        public async Task<IActionResult> SaveCareer(IFormCollection form)
        {
            try
            {
                var client = CreateClients();

                //  Read token from cookie (JWT for backend auth)
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                //  Prepare multipart form data for backend API
                using var multipart = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                {
                    multipart.Add(new StringContent(form[key]), key);
                }

                //  Forward to backend API endpoint
                var response = await client.PostAsync($"{_apiBase}/Career/Post", multipart);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("/Admin/UpdateCareer")]
        public async Task<IActionResult> UpdateCareer(IFormCollection form)
        {
            try
            {
                var client = CreateClients();

                //  Read JWT token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                //  Build multipart form content
                using var multipart = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                {
                    multipart.Add(new StringContent(form[key]), key);
                }

                //  Forward to backend API endpoint
                var response = await client.PutAsync($"{_apiBase}/Career/Update", multipart);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("/Admin/DeleteCareer/{id}")]
        public async Task<IActionResult> DeleteCareer(int id, int updatedBy)
        {
            var client = CreateClients();

            // Attach JWT token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Call backend API
            var response = await client.DeleteAsync($"{_apiBase}/Career/{id}?updatedBy={updatedBy}");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API returned {response.StatusCode}: {result}");
                return StatusCode((int)response.StatusCode, result);
            }

            return Content(result, "application/json");
        }


        public IActionResult UserRole()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles()
        {
            var client = CreateClients();
            var response = await client.GetAsync($"{_apiBase}/UserRole");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpPost("/Admin/SaveUserRole")]
        public async Task<IActionResult> SaveUserRole([FromBody] UserRoleMaster model)
        {
            try
            {
                var client = CreateClients();

                //  Read token from cookie
                if (Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //  Forward to backend API
                var response = await client.PostAsync($"{_apiBase}/UserRole", content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned {response.StatusCode}: {result}");
                    return StatusCode((int)response.StatusCode, result);
                }

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("/Admin/DeleteUserRole/{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            var client = CreateClients();

            // Attach JWT token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Call backend API
            var response = await client.DeleteAsync($"{_apiBase}/UserRole/{id}");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API returned {response.StatusCode}: {result}");
                return StatusCode((int)response.StatusCode, result);
            }

            return Content(result, "application/json");
        }


        public IActionResult UserRolePermission()
        {
            return View();
        }

        [HttpGet("/Admin/GetUserRolesForPermission")]
        public async Task<IActionResult> GetUserRolesForPermission()
        {
            var client = CreateClients();

            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"{_apiBase}/UserRole");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var json = await response.Content.ReadAsStringAsync();

            // convert string → real json object
            var roles = JsonConvert.DeserializeObject<List<UserRoleMaster>>(json);

            return Ok(roles);  // <-- THIS IS THE FIX
        }



        // Proxy user-role-permission list
        [HttpGet("/Admin/GetUserRolePermission")]
        public async Task<IActionResult> GetUserRolePermission()
        {
            var client = CreateClients();

            //  Add Bearer token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            //  Call correct API route (GetAll)
            var res = await client.GetAsync($"{_apiBase}/UserRolePermission");

            var json = await res.Content.ReadAsStringAsync();
              if (!res.IsSuccessStatusCode)
                return StatusCode((int)res.StatusCode, json);

            return Content(json, "application/json");
        }


        [HttpGet("/Admin/GetAllRolePermission")]
        public async Task<IActionResult> GetAllRolePermission()
        {
            var client = CreateClients();

            //  Add Bearer token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            //  Call correct API route (GetAll)
            var res = await client.GetAsync($"{_apiBase}/UserRolePermission/GetAllPermissions");

            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                return StatusCode((int)res.StatusCode, json);

            return Content(json, "application/json");
        }


        [HttpGet("/Admin/GetUserPermission")]
        public async Task<IActionResult> GetUserPermission()
        {
            var client = CreateClients();

            // Pass token to API
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // Call API
            var res = await client.GetAsync($"{_apiBase}/UserRolePermission");
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return StatusCode((int)res.StatusCode, json);

            // IMPORTANT: Convert string → REAL JSON
            var parsed = JsonConvert.DeserializeObject(json);

            return Ok(parsed);  // <--- FIX: JS expects object, not string
        }

        [HttpGet("/Admin/GetUserPermissionById/{id}")]
        public async Task<IActionResult> GetUserPermissionById(int id)
        {
            var client = CreateClients();

            if (Request.Cookies.TryGetValue("AuthToken", out var token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync($"{_apiBase}/UserRolePermission/GetByRoleId/{id}");
            var json = await res.Content.ReadAsStringAsync();

            return StatusCode((int)res.StatusCode, json);
        }



        [HttpPost("/Admin/SaveUserRolePermission")]
        public async Task<IActionResult> SaveUserRolePermission([FromBody] RolePermissionDto model)
        {
            var client = CreateClients();

            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var jsonBody = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{_apiBase}/UserRolePermission", content);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return StatusCode((int)res.StatusCode, json);

            return Ok(JsonConvert.DeserializeObject(json));
        }

        // delete permission by id
        //[HttpDelete("DeleteUserRolePermission/{id}")]
        //public async Task<IActionResult> DeleteUserRolePermission(int id)
        //{
        //    var client = CreateClients();
        //    var res = await client.DeleteAsync($"{_apiBase}/UserRolePermission/{id}");
        //    var json = await res.Content.ReadAsStringAsync();
        //    if (!res.IsSuccessStatusCode) return StatusCode((int)res.StatusCode, json);
        //    return Content(json, "application/json");
        //}

        [HttpDelete("/Admin/DeleteUserRolePermission/{roleId}")]
        public async Task<IActionResult> DeleteUserRolePermission(int roleId)
        {
            var client = CreateClients();

            // Attach JWT token from cookie
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Call backend API
            var response = await client.DeleteAsync($"{_apiBase}/UserRolePermission/{roleId}");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API returned {response.StatusCode}: {result}");
                return StatusCode((int)response.StatusCode, result);
            }

            return Content(result, "application/json");
        }


    }
}
