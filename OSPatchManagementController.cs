using Identity.Common.Implementation.Models;
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
    public class OSPatchManagementController : Controller
    {
        private readonly ILogger<OSPatchManagementController> _logger;
        private readonly IConfiguration _configuration;
        private EndagentAPI eaAPI;
        private readonly string AddUpdatePatchesinQueue;
        private readonly string LoadOSPatches;
        private readonly string UploadFile;
        private readonly string GetEligiblePatches;
        private readonly string GetPatchLogs;
        private readonly string getPatchUpdateStatus;
        private readonly string getTotalAvailableAndInstalledPatchCount;
        private readonly string getInstalledPatches;
        private readonly string getAvailablePatches;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();


        public OSPatchManagementController(ILogger<OSPatchManagementController> logger, IConfiguration configuration, EndagentAPI _eaAPI, ITokenExtraction tokenExtraction)
        {
            _logger = logger;
            _configuration = configuration;
            AddUpdatePatchesinQueue = configuration.GetValue<string>("ApiEndPoints:AddUpdatePatchesinQueue");
            LoadOSPatches = configuration.GetValue<string>("ApiEndPoints:LoadPatches");
            UploadFile = configuration.GetValue<string>("ApiEndPoints:UploadFile");
            GetPatchLogs = configuration.GetValue<string>("ApiEndPoints:GetPatchLogs");
            GetEligiblePatches = configuration.GetValue<string>("ApiEndPoints:GetEligiblePatches");
            getPatchUpdateStatus = configuration.GetValue<string>("ApiEndPoints:getPatchUpdateStatus");
            getTotalAvailableAndInstalledPatchCount = configuration.GetValue<string>("ApiEndPoints:AvailableInstalledPatchCount");
            getInstalledPatches = configuration.GetValue<string>("ApiEndPoints:GetInstalledPatches");
            getAvailablePatches = configuration.GetValue<string>("ApiEndPoints:GetAvailablePatches");
            eaAPI = _eaAPI;
            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
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
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                string? token = await HttpContext.GetTokenAsync("access_token");

                var patches = await eaAPI.GetEligiblePatches(orgId, GetEligiblePatches, token);
                ViewBag.SystemIds = patches.Select(p => p.SystemID).Distinct().ToList(); // VL ma'am

                return View(patches);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest(ex.Message);
            }

        }

        public async Task<IActionResult> PatchDetails(string type, string id)
        {
            ViewBag.FirstName = _tokenProperty.FirstName;
            ViewBag.LastName = _tokenProperty.LastName;
            ViewBag.Email = _tokenProperty.Email;
            ViewBag.Username = _tokenProperty.Name;
            string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
            string? token = await HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(id))
            {
                var model = new OSPatches();
                model.OSPatchesCount = new OSPatchesCount()
                {
                    AvailablePatchesCount = 0,
                    InstalledPatchesCount = 0
                };
                // Get device info (assuming you have a method to get device by id)
               
                // Always load both counts
                var available = await GetAvailableOrInstalledPatches(id, getAvailablePatches, "kb");
                var installed = await GetAvailableOrInstalledPatches(id, getInstalledPatches, "kb");
                if (available != null)
                {
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

                    if (type == "availablepatches")
                    {
                        model.AvailablePatches = available.AvailablePatches;
                    }
                    else if (type == "installedpatches")
                    {
                        model.InstalledPatches = installed.InstalledPatches;
                    }
                }


                if (type == "availablepatches")
                {
                    ViewBag.Tab = "availablepatches";
                }
                else if (type == "installedpatches")
                {
                    ViewBag.Tab = "installedpatches";
                }
                else
                {
                    ViewBag.Tab = "urlbroken";
                }


                return View(model);
            }

            return View(null);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitPatches(List<string> SelectedPatches, DateTime? InstallDateTime)
        {
            try
            {
                if (!InstallDateTime.HasValue)
                {
                    InstallDateTime = DateTime.Now;
                }

                string? token = await HttpContext.GetTokenAsync("access_token");

                if (SelectedPatches == null || !SelectedPatches.Any())
                {
                    return BadRequest("No patches selected.");
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

                var response = await eaAPI.AddinUpdatePatchQueue(patches, AddUpdatePatchesinQueue, token);

                if (response)
                {
                    TempData["ResultMessage"] = "Patches successfully added to queue.";
                    TempData["AlertType"] = "success";
                }
                else
                {
                    TempData["ResultMessage"] = "Failed to add patches to queue. Please try again.";
                    TempData["AlertType"] = "danger";
                }

                return RedirectToAction(
                    "PatchDetails",
                    new { type = "availablepatches", id = patches[0].SystemID }
                );


            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult AddPatches()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                _logger.LogInformation("AddPatches View");
                return View("AddPatches");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> AddPatches([FromBody] FileUploadDTO file)
        {
            try
            {
                string role = _tokenProperty.Role;
                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")                
                {
                    _logger.LogInformation("AddPatches: AccessDenied");
                    return View("AccessDenied");
                }
                
                file.OrgID = _tokenProperty.OrgId;
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(_tokenProperty.Token) as JwtSecurityToken;
                var tokenExpiration = jwtToken.ValidTo;

                if (tokenExpiration < DateTime.UtcNow)
                {
                    return RedirectToAction("Logout", "Account");
                }

                if (file.UploadedFileBase64 == null || file.UploadedFileBase64.Length == 0)
                {
                    ModelState.AddModelError("File", "Please upload a valid file.");
                    return View("AddPatches", file);
                }

                bool result = await eaAPI.UploadFile(file, _tokenProperty.Token, UploadFile);
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
                _logger.LogInformation(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        public async Task<List<FileUploadDTO>> LoadPatches()
        {
            try
            {
                var files = await eaAPI.LoadPatches(_tokenProperty.OrgId, LoadOSPatches, _tokenProperty.Token);
                if(files!=null)
                    files = files.Where(f => f.KBNumber != "app").ToList();

                return files;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);

                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePatchLogs()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;

                var files = await eaAPI.GetPatchLogs(_tokenProperty.OrgId, GetPatchLogs, _tokenProperty.Token);
                files = files.Where(f => f.UpdateKBNumber != "app").ToList();

                return View(files);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest();
                
            }
        }

        // VL ma'am
        [HttpGet]
        public async Task<IActionResult> GetPatchStatus(string objId)
        {
            try
            {
                string token = await HttpContext.GetTokenAsync("access_token");

                var resultJson = await eaAPI.GetPatchStatusAsync(objId, getPatchUpdateStatus, token);

                // Return JSON
                return Content(resultJson, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching patch status", error = ex.Message });
            }
        }
               
        private async Task<OSPatches> GetAvailableOrInstalledPatches(string assetId, string url,string kbtype)
        {
            string? _token = await HttpContext.GetTokenAsync("access_token");            
            var _getavailablepatches = await eaAPI.GetAvailableOrInstalledPatches(_tokenProperty.OrgId, url,kbtype, assetId, _token);
            return _getavailablepatches;
        }        
    }
}
