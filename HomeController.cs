using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Monitoring.Models;
using Monitoring.Models.DTO;
using Newtonsoft.Json;
using NuGet.ContentModel;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Identity.Common.Implementation.Models;
using Microsoft.EntityFrameworkCore;
using Monitoring.Data;

namespace Monitoring.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string ITAMApiKeyHeader;
        private readonly string ITAMApiKey;
        private readonly string eaApiKeyHeader;
        private readonly string eaApiKey;
        private readonly string loginStatusUrl;
        private readonly string baseUrl;
        private readonly string getDevicesUrl;
        private readonly string checkBIOSUrl;
        private readonly string getBIOSurl;
        private EndagentAPI eaAPI;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();
        private readonly SSODBContext _dbContext;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ITokenExtraction tokenExtraction, EndagentAPI _eaAPI, SSODBContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            ITAMApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:ITAMApiKeyHeader");
            ITAMApiKey = configuration.GetValue<string>("ApiEndPoints:ITAMApiKey");
            eaApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:eaApiKeyHeader");
            eaApiKey = configuration.GetValue<string>("ApiEndPoints:eaApiKey");
            loginStatusUrl = configuration.GetValue<string>("ApiEndPoints:loginStatusUrl");
            baseUrl = _configuration.GetValue<string>("ApiEndPoints:baseUrl");
            getDevicesUrl = _configuration.GetValue<string>("ApiEndPoints:getDevicesUrl");
            checkBIOSUrl = _configuration.GetValue<string>("ApiEndPoints:checkBIOSUrl");
            getBIOSurl = _configuration.GetValue<string>("ApiEndPoints:getBIOSurl");
            eaAPI = _eaAPI;
            _tokenExtraction = tokenExtraction;
            _tokenProperty=_tokenExtraction.ExtractToken().GetAwaiter().GetResult();
            _dbContext = dbContext;
        }

        public IActionResult Dash()
        {
            ViewBag.FirstName = _tokenProperty.FirstName;
            ViewBag.LastName = _tokenProperty.LastName;
            ViewBag.Email = _tokenProperty.Email;
            ViewBag.Username = _tokenProperty.Name;
            string role = _tokenProperty.Role;
            return View();
        }

        public async Task<JsonResult> GetKpis()
        {
            string _url = _configuration.GetValue<string>("ApiEndPoints:kpis");
            var _result = await eaAPI.GetPatchKPIs(_tokenProperty.OrgId, _url, _tokenProperty.Token);
            if(_result != null )                
                return Json( _result);

            return Json(new KPISViewModel());
        }

        public async  Task<IActionResult> Index()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string role = _tokenProperty.Role;                

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || (role != "Admin" && role != "SuperAdmin" && role != "SuperAdmin2"))
                {
                    return RedirectToAction("AccessDenied", "Account");

                }

                // Dev: Viraj; Date: 09-05-2024
                if (role == "SuperAdmin2")
                {
                    return RedirectToAction("AgentManager","Workstation");
                }

                string? token = _tokenProperty.Token;

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var tokenExpiration = jwtToken?.ValidTo;

                if (tokenExpiration < DateTime.UtcNow)
                {
                    // If the access token has expired, redirect the user to the logout action
                    return RedirectToAction("Logout", "Account");
                }

                return View();
            }
            catch(Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation(ex,$"User: {user}");
                return NotFound();
            }
        }

        public IActionResult GetApiDetails()
        {
            string user = _tokenProperty.Name;
            var apiUrl = $"{loginStatusUrl}/{user}";
            var apiKey = ITAMApiKeyHeader;
            var apiValue = ITAMApiKey;

            var apiDetails = new ApiDetails
            {
                ApiUrl = apiUrl,
                ApiKey = apiKey,
                ApiValue = apiValue
            };

            return Json(apiDetails);
        }

        public IActionResult Privacy()
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                return View();
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return NotFound();
            }
        }

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            string role = _tokenProperty.Role;

            // Now 'values' should contain ["admin", "5"]

            if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
            {
                return View("AccessDenied");

            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> GetLoginStatus()
        {
            return Json("true");
            //string user = _tokenProperty.Name;

            //try
            //{
            //    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //    //Donot Use in Production (after Implementing SSL)
            //    HttpClientHandler handler = new HttpClientHandler();

            //    // Disable SSL/TLS validation by accepting all certificates
            //    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            //    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //    using (HttpClient httpClient = new HttpClient(handler))
            //    {
            //        httpClient.DefaultRequestHeaders.Add(ITAMApiKeyHeader, ITAMApiKey);

            //        HttpResponseMessage response = await httpClient.GetAsync($"{loginStatusUrl}/{user}");
            //        if (response.IsSuccessStatusCode)
            //        {
            //            string responseContent = await response.Content.ReadAsStringAsync();
            //            if (responseContent != null)
            //            {
            //                string IsOnline = JsonConvert.DeserializeObject<string>(responseContent);
            //                return Json(IsOnline);
            //            }
            //        }
            //        return Json("none");
            //    }
            //}
            //catch (Exception ex)
            //{

            //    return Json("none");
            //}
        }

        // Home screen Pi Chart 1
        [HttpGet]
        public async Task<IActionResult> GetApprovedVsUnapprovedDevices()
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                    return RedirectToAction("AccessDenied", "Account");

                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                List<DeviceDTO> eaDevices = await eaAPI.GetDevices(orgId, user, baseUrl, getDevicesUrl, eaApiKeyHeader, token);
                
                if (eaDevices == null || eaDevices.Count() == 0)
                {
                    _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                    return Json(eaDevices);
                }
                
                int approved = eaDevices.Where(d => d.IsApproved == true).ToList().Count();
                int unApproved = eaDevices.Where(d => d.IsApproved == false).ToList().Count();
                List<int> result = new List<int>();
                result.Add(approved);
                result.Add(unApproved);

                return Json(result);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        // Home Screen Pi Chart 2
        [HttpGet]
        public async Task<IActionResult> GetInITAMvsOutITAM()
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");
                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token; 

                List<DeviceDTO> eaDevices = await eaAPI.GetDevices(orgId, user, baseUrl, getDevicesUrl, eaApiKeyHeader, token);
                if (eaDevices == null)
                {
                    //return NotFound(new { message = "eaDevices API error." });
                    return Json(eaDevices);
                }
                eaDevices = eaDevices.Where(a => a.IsApproved == true).ToList();
                // Dev: Viraj; Date:24-05-2024; To get data in real time from ITAM Registry
                int inITAM = 0;
                int outITAM = 0;
                foreach (var device in eaDevices)
                {
                    BIOSDetailsDTO model = await eaAPI.GetBIOS(device.ID.ToString(), token, getBIOSurl);
                    ITAMBIOSDetailsDTO temp = new ITAMBIOSDetailsDTO();
                    temp.AssetID = "";
                    if (model != null)
                    {
                        temp.BIOS_SN = model.BIOS;
                        temp.TrackerID = model.ID;
                    }
                    string check = await eaAPI.CheckBIOS(temp, orgId, checkBIOSUrl, ITAMApiKeyHeader, ITAMApiKey);
                    if (check != null)
                    {
                        if (check.Contains("exists")||check.Contains("link"))
                        {
                            inITAM++;
                        }
                        else if (check.ToString() == "false")
                        {
                            outITAM++;
                        }
                    }
                }
                List<int> result = new List<int>
                {
                    inITAM,
                    outITAM
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Signout()
        {
            // Retrieve ID token for logout request
            var idToken = await HttpContext.GetTokenAsync("id_token");

            //Construct the IdentityServer logout URL
            var logoutUri = $"{_configuration["SSOServer:Login"]}/connect/endsession";

            if (!string.IsNullOrEmpty(idToken))
            {
                logoutUri += $"?id_token_hint={idToken}";
            }

            // Redirect to IdentityServer for global logout
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = logoutUri
            }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

        }

        // this function is used to navigation to all application i.e CommonNavigation
        public IActionResult Navigation(string id)
        {
            bool flag = GetPermissions(_tokenProperty.OrganizationId).Result.Any(x => x.ApplicationId == Convert.ToInt32(id) && x.ApplicationPermission == true);
            if (flag)
            {
                string _url = GetAppsUrls(id);
                if (string.IsNullOrEmpty(_url) || _url == "#")
                {
                    return RedirectToAction("PageUnderContruction");
                }
                else
                {
                    //WorkStationHub
                    if (id == "2")
                    {
                        if ((new string[] { "SuperAdmin", "Admin", "Auditor", "Manager", "User" }).Contains(_tokenProperty.Role))
                            return Redirect(_url);
                        else
                            return RedirectToAction("AccessDenied", "Account");
                    }

                    //ITSM or ITSMEndUser
                    if (id == "3")
                    {
                        if ((new string[] { "SuperAdmin", "Admin", "Manager", "L1User", "L2User", "User", "EndUser" }).Contains(_tokenProperty.Role))
                        {
                            if (_tokenProperty.Role != "EndUser")
                            {
                                return Redirect(_url);
                            }
                            else
                            {
                                // this will call only if login user role is enduser 
                                return Redirect(_configuration.GetValue<string>("Access:ITSMEndUserUrl")!);
                            }
                        }
                        else
                            return RedirectToAction("AccessDenied", "Account");
                    }

                    // 4 => NonIT | 5 => Monitoring | 8 => Patch Management
                    if ((new string[] {"4", "5", "6", "7", "8" }).Contains(id))
                    {
                        if ((new string[] { "SuperAdmin", "Admin", "Manager", "User" }).Contains(_tokenProperty.Role))
                            return Redirect(_url);
                        else
                            return RedirectToAction("AccessDenied", "Account");
                    }
                }
            }

            return RedirectToAction("AccessDenied", "Account");
        }

        private async Task<List<AppsPermissionViewModel>> GetPermissions(int orgId)
        {
            return await _dbContext.ApplicationLists
                                               .Join(_dbContext.ApplicationPermissions, apps => apps.AppsListId, perm => perm.AppsId, (apps, perm) => new { apps, perm })
                                               .Where(_ => _.perm.OrgId == Convert.ToInt32(orgId))
                                               .OrderBy(x => x.apps.AppsListId)
                                               .Select(x => new AppsPermissionViewModel()
                                               {
                                                   ApplicationId = x.apps.AppsListId,
                                                   ApplicationName = x.apps.AppsListName,
                                                   ApplicationPermission = x.perm.IsActive,
                                                   OrganizationId = x.perm.OrgId,
                                                   DisplayName = x.apps.DisplayName,
                                                   Description = x.apps.Description,
                                                   Icons = x.apps.Icons,

                                               }).ToListAsync<AppsPermissionViewModel>();

        }

        private string GetAppsUrls(string appsId)
        {
            string _targetUrl = string.Empty;
            if (!string.IsNullOrEmpty(appsId))
            {
                switch (appsId)
                {
                    case "2":
                        _targetUrl = _configuration.GetValue<string>("Access:ITAMUrl") ?? string.Empty;
                        break;
                    case "3":
                        _targetUrl = _configuration.GetValue<string>("Access:ITSMUrl") ?? string.Empty;
                        break;
                    case "4":
                        _targetUrl = _configuration.GetValue<string>("Access:NonITAMUrl") ?? string.Empty;
                        break;
                    case "5":
                        _targetUrl = _configuration.GetValue<string>("Access:Monitoring") ?? string.Empty;
                        break;
                    case "6":
                        _targetUrl = _configuration.GetValue<string>("Access:PatchManagement") ?? string.Empty;
                        break;
                    case "8":
                        _targetUrl = _configuration.GetValue<string>("Access:PatchManagement") ?? string.Empty;
                        break;
                    default:
                        _targetUrl = string.Empty;
                        break;
                }
            }

            return _targetUrl;
        }

    }
}