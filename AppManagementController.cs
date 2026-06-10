using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.Controllers.Devices;
using Monitoring.Models;
using Monitoring.Models.DTO;
using Monitoring.ViewModel;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Monitoring.Controllers
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AppManagementController : Controller
    {
        private readonly ILogger<AppManagementController> _logger;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();
        private readonly IConfiguration _configuration;
        private EndagentAPI eaAPI;
        private readonly string GetEligibleApps;
        private readonly string AddUpdatePatchesinQueue;
        private readonly string AddSoftwareUpdatePatchesinQueue;
        private readonly string LoadPatches;
        private readonly string GetPatchLogs;
        private readonly string UploadFile;
        private readonly string getAvailablePatches;
        private readonly string getInstalledSoftwares;

        public AppManagementController(ILogger<AppManagementController> logger, IConfiguration configuration, ITokenExtraction tokenExtraction, EndagentAPI _eaAPI)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenExtraction = tokenExtraction;
            GetEligibleApps = configuration.GetValue<string>("ApiEndPoints:GetEligibleApps");
            AddUpdatePatchesinQueue = configuration.GetValue<string>("ApiEndPoints:AddUpdatePatchesinQueue");
            AddSoftwareUpdatePatchesinQueue= configuration.GetValue<string>("ApiEndPoints:AddSoftwareUpdatePatchesinQueue");
            LoadPatches = configuration.GetValue<string>("ApiEndPoints:LoadPatches");
            GetPatchLogs = configuration.GetValue<string>("ApiEndPoints:GetPatchLogs");
            UploadFile = configuration.GetValue<string>("ApiEndPoints:UploadFile");
            getAvailablePatches = configuration.GetValue<string>("ApiEndPoints:GetAvailablePatches");
            getInstalledSoftwares = configuration.GetValue<string>("ApiEndPoints:GetInstalledSoftwares");
            _tokenProperty =_tokenExtraction.ExtractToken().GetAwaiter().GetResult();
            eaAPI = _eaAPI;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? id)
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string orgId = _tokenProperty.OrgId;
                string token = _tokenProperty.Token;

                var patches = await eaAPI.GetEligiblePatches(orgId, GetEligibleApps, token);
                return View(patches);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured");
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> AppDetails(string type, string id)
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string orgId = _tokenProperty.OrgId;
                string token = _tokenProperty.Token;

                if (!string.IsNullOrEmpty(id))
                {
                    var model = new OSPatches();

                    // Always load counts
                    var available = await GetAvailableOrInstalledPatches(id, getAvailablePatches, "app");
                    var installed = await GetAvailableOrInstalledPatches(id, getInstalledSoftwares, "app");
                    model = new OSPatches
                    {
                        SystemID = available.SystemID,
                        DeviceName = available.DeviceName,
                        DeviceBIOS = available.DeviceBIOS,
                        User = available.User,
                        OrgID = available.OrgID
                    };
                    model.OSPatchesCount = new OSPatchesCount
                    {
                        AvailablePatchesCount = available.OSPatchesCount.AvailablePatchesCount,
                        InstalledPatchesCount = installed.OSPatchesCount.InstalledPatchesCount
                    };

                    if (type == "availablesoftwares")
                    {
                        ViewBag.Tab = "availablesoftwares";
                        model.AvailablePatches = available.AvailablePatches;
                    }
                    else if (type == "installedsoftwares")
                    {
                        ViewBag.Tab = "installedsoftwares";
                        model.InstalledApps = installed.InstalledApps;
                    }
                    else
                    {
                        ViewBag.Tab = "urlbroken";
                    }

                    return View(model);
                }

                return View(null);

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured");
                return BadRequest(ex.Message);
            }

        }


        [HttpPost]
        public async Task<IActionResult> SubmitPatches(List<string> SelectedPatches, DateTime? InstallDateTime)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    // no implementation - to avoid warning
                }

                if (!InstallDateTime.HasValue)
                {
                    InstallDateTime = DateTime.Now;
                }

                string? token = _tokenProperty.Token;

                if (SelectedPatches == null || !SelectedPatches.Any())
                {
                    return BadRequest("No software selected.");
                }

                List<UpdatePatchQueueTempDTO> patches = new List<UpdatePatchQueueTempDTO>();
                foreach (var selectedPatch in SelectedPatches)
                {
                    UpdatePatchQueueTempDTO patch = new UpdatePatchQueueTempDTO();
                    var parts = selectedPatch.Split('|');
                    patch.SystemID = parts[0];
                    patch.PatchID = parts[1];
                    patch.ScheduleTime = InstallDateTime;
                    patch.OrgId = _tokenProperty.OrgId;
                    patches.Add(patch);
                }

                var response = await eaAPI.AddinUpdatePatchQueue(patches, AddSoftwareUpdatePatchesinQueue, token);

                if (response)
                {
                    TempData["ResultMessage"] = "Software successfully added to queue.";
                    TempData["AlertType"] = "success";
                }
                else
                {
                    TempData["ResultMessage"] = "Failed to add software to queue. Please try again.";
                    TempData["AlertType"] = "danger";
                }

                //return RedirectToAction("Index");
                return RedirectToAction(
                    "AppDetails",
                    new { type = "availablesoftwares", id = patches[0].SystemID }
                );
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex,"error occured");
                return BadRequest(ex.Message);
            }
        }

        public async Task<List<FileUploadDTO>> LoadApps()
        {
            try
            {
                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;

                var files = await eaAPI.LoadPatches(orgId, LoadPatches, token);
                files = files.Where(f => f.KBNumber == "app").ToList();
                return files;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> AppLogs()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;


                var files = await eaAPI.GetPatchLogs(orgId, GetPatchLogs, token);
                files = files.Where(f => f.UpdateKBNumber == "app").ToList();
                return View(files);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult AddApps()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                return View("AddApps"); 

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddApps([FromBody] FileUploadDTO file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // no implementation - to avoid warning
                }

                string role = _tokenProperty.Role;

                if (string.IsNullOrEmpty(role)|| (role != "Admin" && role != "SuperAdmin"))
                {
                    return View("AccessDenied");
                }

                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;
                file.OrgID = orgId;
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var tokenExpiration = jwtToken?.ValidTo;

                if (tokenExpiration < DateTime.UtcNow)
                {
                    return RedirectToAction("Logout", "Account");
                }

                if (file.UploadedFileBase64 == null || file.UploadedFileBase64.Length == 0)
                {
                    ModelState.AddModelError("File", "Please upload a valid file.");
                    return View("AddApps", file);
                }

                bool result = await eaAPI.UploadFile(file, token, UploadFile);
                if (result)
                {
                    ViewBag.Result = "success";
                    return Json("success"); // Return 'success' if upload succeeds
                }
                ViewBag.Result = "fail";
                return Json("fail"); // Return 'fail' if upload fails

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured");
                return BadRequest(ex.Message);
            }
        }

        private async Task<OSPatches> GetAvailableOrInstalledPatches(string assetId, string url, string kbtype)
        {
            string? _token = await HttpContext.GetTokenAsync("access_token");
            var _getavailablepatches = await eaAPI.GetAvailableOrInstalledPatches(_tokenProperty.OrgId, url, kbtype, assetId, _token);
            return _getavailablepatches;
        }
    }
}
