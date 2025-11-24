using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Plugins;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase;

        public AuthController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiBase = $"{apiSettings.Value.BaseUrl}";
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

        private readonly string _apiBaseUrl;
        private readonly string _apiOrigin;
        private readonly IHttpClientFactory _clientFactory;


        public AuthController(IOptions<ApiSettings> apiSettings, IHttpClientFactory clientFactory)
        {
            _apiBaseUrl = apiSettings.Value.BaseUrl;
            _apiOrigin = apiSettings.Value.Origin;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new Register());
        }

        // POST: submit form to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var client = _httpClientFactory.CreateClient("IgnoreSSL");

            try
            {
                var response = await client.PostAsJsonAsync($"{_apiBase}/Register", model);
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/Register", model);

                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Registration successful!" });
                }

                return Json(new { success = false, message = "Registration Unsuccessful! Please try another credentials." + content });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // CALL LOGIN API HERE
        [HttpPost]
        public async Task<IActionResult> LoginWithPassword(string contact_number, string password)
        {
            if (string.IsNullOrWhiteSpace(contact_number) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Contact number and password are required.");
                return RedirectToAction(nameof(Login));
            }

            //using var client = new HttpClient();

            var client = _clientFactory.CreateClient("IgnoreSSL");

            var client = _httpClientFactory.CreateClient("IgnoreSSL");

           
            var loginRequest = new LoginRequest
            {
                ContactNumber = contact_number,
                Password = password
            };

            try
            {

                var response = await client.PostAsJsonAsync($"{_apiBase}/Login/password", loginRequest);
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/Login/password", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(stream);

                    // Extract userRole directly from JSON
                    var root = doc.RootElement;

                    var token = root.TryGetProperty("token", out var tokenProp)
                ? tokenProp.GetString()
                : null;

                    var loginId = root.GetProperty("user").GetProperty("id").GetInt32();
                    var userRole = root.GetProperty("user").GetProperty("userRole").GetString();
                    var userRoleId = root.GetProperty("user").GetProperty("userRoleId").GetInt32();
                    var emailId = root.GetProperty("user").GetProperty("email").GetString();
                    var contactNumber = root.GetProperty("user").GetProperty("contact_number").GetString();
                    var firstName = root.GetProperty("user").GetProperty("firstName").GetString();
                    var lastName = root.GetProperty("user").GetProperty("lastName").GetString();

                    var fullName = $"{firstName} {lastName}".Trim();

                    if (!string.IsNullOrEmpty(token))
                    {
                        Response.Cookies.Append("AuthToken", token, new CookieOptions
                        {
                            HttpOnly = true, // not accessible by JS
                            Secure = true,   // only HTTPS
                            SameSite = SameSiteMode.None,
                            Expires = DateTimeOffset.UtcNow.AddHours(12)
                        });
                    }

                    //if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                    //{
                    //    // find the cookie for InternalPortalAuth
                    //    var setCookie = setCookieHeaders.FirstOrDefault(h => h.StartsWith("InternalPortalAuth="));
                    //    if (!string.IsNullOrEmpty(setCookie))
                    //    {
                    //        var cookieValue = setCookie.Split(';', 2)[0].Split('=', 2)[1];

                    //        Response.Cookies.Append("InternalPortalAuth", cookieValue, new CookieOptions
                    //        {

                    //            HttpOnly = true,
                    //            Secure = true,
                    //            SameSite = SameSiteMode.None,
                    //            Expires = DateTimeOffset.UtcNow.AddDays(1)
                    //        });
                    //    }
                    //}

                    if (loginId > 0)
                    {
                        HttpContext.Response.Cookies.Append("LoginId", loginId.ToString(), new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });

                    var fullName = $"{firstName} {lastName}".Trim();

                    if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                    {
                        // find the cookie for InternalPortalAuth
                        var setCookie = setCookieHeaders.FirstOrDefault(h => h.StartsWith("InternalPortalAuth="));
                        if (!string.IsNullOrEmpty(setCookie))
                        {
                            // value part: "InternalPortalAuth=COOKIEVALUE; Path=/; HttpOnly; ..."
                            var cookieValue = setCookie.Split(';', 2)[0].Split('=', 2)[1];

                            // Save the cookie for the browser (so it will be available in HttpContext.Request.Cookies on next request)
                            Response.Cookies.Append("InternalPortalAuth", cookieValue, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddDays(1)
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(userRole))
                    {
                        HttpContext.Response.Cookies.Append("UserRole", userRole, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                    }

                    if (userRoleId > 0)
                    {
                        HttpContext.Response.Cookies.Append("UserRoleId", userRoleId.ToString(), new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                    }

                    if (!string.IsNullOrEmpty(emailId))
                    {
                        HttpContext.Response.Cookies.Append("Email", emailId, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                    }

                    if (!string.IsNullOrEmpty(emailId))
                    {
                        HttpContext.Response.Cookies.Append("Email", emailId, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                    }

                    if (!string.IsNullOrEmpty(contactNumber))
                    {
                        HttpContext.Response.Cookies.Append("ContactNumber", contactNumber, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                    }

                    if (!string.IsNullOrEmpty(fullName))
                    {
                        HttpContext.Response.Cookies.Append("FullName", fullName, new CookieOptions
                        {
                            HttpOnly = true,   // Prevent JS access
                            Secure = true,     // Send only over HTTPS
                            SameSite = SameSiteMode.None, // Protect from CSRF
                            SameSite = SameSiteMode.Strict, // Protect from CSRF
                            Expires = DateTimeOffset.UtcNow.AddDays(1) // Expiry time
                        });
                    }

                    TempData["SuccessMessage"] = "";
                    //return RedirectToAction("Index", "Company");
                    switch (userRole.ToLower())
                    {
                        case "admin":
                            return RedirectToAction("Index", "Company");
                        case "employee":
                            return RedirectToAction("Index", "Employee");
                        case "customer":
                            return RedirectToAction("CustomerDashboard", "Customer");
                        case "company":
                            return RedirectToAction("CompanyDashboard", "Company");
                        case "vendor":
                            return RedirectToAction("VendorDashboard", "Vendor");
                            return RedirectToAction("Index", "Customer");
                        case "company":
                            return RedirectToAction("Index", "Company");
                        case "vendor":
                            return RedirectToAction("Index", "Vendor");
                        default:
                            return RedirectToAction("Index", "Home"); // default
                    }

                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError("", "Invalid contact number or password.");
                    return RedirectToAction(nameof(Login));
                }

                // Handle other errors
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"API error ({(int)response.StatusCode}): {error}");
                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", "Error contacting API: " + ex.Message);
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unexpected error: " + ex.Message);
                return RedirectToAction(nameof(Login));
            }
        }

        [HttpPost]
        public async Task<IActionResult> LoginwithOTP([FromBody] OtpRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.contact_number))
                return BadRequest(new { message = "Mobile number is required." });

            var client = CreateClients();

            // If the downstream API expects { contact_number: "..." } you can send model directly.
            var response = await client.PostAsJsonAsync($"{_apiBase}/Login/request-otp", model);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                // return status from downstream as-is
                return StatusCode((int)response.StatusCode, err);
            }

            var json = await response.Content.ReadAsStringAsync();
            // return the raw JSON the API gave you
            return Content(json, "application/json");
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOTP([FromBody] OtpVerifyRequest model)
        {
            var client = CreateClients();

            var response = await client.PostAsJsonAsync(
                $"{_apiBase}/Login/verify-otp",
                model
            );

            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Content(jsonResponse, "application/json"); // return error JSON

            return Content(jsonResponse, "application/json"); // return success JSON
        }

        [HttpPost]
        public IActionResult VerifyOtpSuccess([FromBody] JsonElement root)
        public IActionResult Logout()
        {
            // If no user object → OTP failed
            if (!root.TryGetProperty("user", out JsonElement user))
            {
                TempData["ErrorMessage"] = root.TryGetProperty("message", out var msg)
                    ? msg.GetString()
                    : "OTP verification failed.";

                return RedirectToAction("Login");
            }

            string token = root.TryGetProperty("token", out var tokenProp) ? tokenProp.GetString() : null;

            int loginId = user.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
            string userRole = user.TryGetProperty("userRole", out var roleProp) ? roleProp.GetString() : "";
            int userRoleId = user.TryGetProperty("userRoleId", out var roleIdProp) ? roleIdProp.GetInt32() : 0;

            string emailId = user.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : "";
            string contactNumber = user.TryGetProperty("contact_number", out var contactProp) ? contactProp.GetString() : "";

            string firstName = user.TryGetProperty("firstName", out var fnProp) ? fnProp.GetString() : "";
            string lastName = user.TryGetProperty("lastName", out var lnProp) ? lnProp.GetString() : "";

            string fullName = $"{firstName} {lastName}".Trim();

            // Set Cookies
            if (!string.IsNullOrEmpty(token))
                Response.Cookies.Append("AuthToken", token);

            Response.Cookies.Append("LoginId", loginId.ToString());
            Response.Cookies.Append("UserRoleId", userRoleId.ToString());
            Response.Cookies.Append("ContactNumber", contactNumber);
            Response.Cookies.Append("Email", emailId);
            Response.Cookies.Append("FullName", fullName);

            // Redirect based on role
            return RedirectAfterLogin(userRole);
        }

        private IActionResult RedirectAfterLogin(string role)
        {
            switch (role.ToLower())
            {
                case "admin": return RedirectToAction("Index", "Company");
                case "employee": return RedirectToAction("Index", "Employee");
                case "customer": return RedirectToAction("CustomerDashboard", "Customer");
                case "company": return RedirectToAction("CompanyDashboard", "Company");
                case "vendor": return RedirectToAction("VendorDashboard", "Vendor");
                default: return RedirectToAction("Index", "Home");
            }
        }



        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("LoginId");
            Response.Cookies.Delete("UserRole");
            Response.Cookies.Delete("ContactNumber");
            Response.Cookies.Delete("Email");
            Response.Cookies.Delete("FullName");
            Response.Cookies.Delete("UserRoleId");
            return RedirectToAction("Index", "Home");
        }
    }

    public class OtpRequest
    {
        public string contact_number { get; set; }
    }

    public class OtpVerifyRequest
    {
        public string contact_number { get; set; }
        public string OTP { get; set; }
    }


    public class LoginRequest
    {
        [JsonPropertyName("contact_number")]
        public string ContactNumber { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

}